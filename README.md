# NapArona

基于 ASP.NET Core 的 OneBot 机器人框架，底层协议处理由 [NapPlana.NET](https://github.com/napplana/NapPlana.NET) 提供。

NapArona 的核心思路是把写 QQ 机器人变成写 Controller —— 和写 Web API 一样的体验，用特性标记方法，框架负责路由和参数绑定。

## 快速开始

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNapArona(opts =>
{
    opts.WebSocketPath = "/onebot";
    opts.AuthenticateAsync = async ctx =>
        ctx.Request.Headers.Authorization.ToString() == "Bearer your-secret-token";
});
builder.Services.AddNapAronaControllers();

var app = builder.Build();
app.UseWebSockets();
app.MapNapArona();
app.UseNapAronaControllers();
app.Run();
```

NapCat 以反向 WebSocket 模式连接到这个服务，连接建立后框架自动完成会话识别和事件订阅。

## 控制器

继承 `BotController`，用 `[Command]` 标记方法即可处理命令。控制器是 Transient 的，每次命令触发都会从 DI 容器创建新实例。

```csharp
public class MyController : BotController
{
    [Command("/ping")]
    public async Task Ping()
    {
        await ReplyTextAsync("pong!");
    }

    [Command("/greet")]
    public async Task Greet(string name, int count = 1)
    {
        for (int i = 0; i < count; i++)
            await ReplyTextAsync($"你好，{name}！");
    }
}
```

命令文本中空格分隔的部分会按顺序绑定到方法参数上，支持 `string`、`int`、`long`、`double`、`bool` 的自动转换，也支持默认值和 `params string[]`。

## 正则命令

`[Command]` 设置 `IsRegex = true` 后，用正则表达式匹配消息，命名捕获组自动绑定到同名参数：

```csharp
[Command(@"^/roll (?<count>\d+)d(?<sides>\d+)$", IsRegex = true)]
public async Task Roll(int count, int sides)
{
    // count 和 sides 从正则命名组中提取并转换为 int
}
```

方法参数声明为 `Match` 类型时，可以拿到完整的正则匹配结果。

## 事件处理

除了消息命令，还可以用 `[OnEvent]` 或预定义的事件特性监听 OneBot 通知事件：

```csharp
[OnGroupPoke]
public async Task OnPoke()
{
    await ReplyTextAsync("别戳我！");
}

[OnGroupIncrease]
public async Task OnNewMember(GroupIncreaseNoticeEvent ev)
{
    await ReplyTextAsync($"欢迎 {ev.UserId} 加入群聊！");
}
```

预定义特性包括 `[OnGroupPoke]`、`[OnFriendPoke]`、`[OnGroupIncrease]`、`[OnGroupDecrease]`，也可以直接用 `[OnEvent(typeof(...))]` 监听任意事件类型。

方法参数声明为对应的事件类型时，框架会自动注入事件对象。

## 过滤器

`[GroupOnly]` 和 `[PrivateOnly]` 可以标记在方法或整个控制器类上，限制命令的触发场景：

```csharp
[Command("/secret")]
[PrivateOnly]
public async Task Secret() { /* 只在私聊中响应 */ }
```

## 授权

使用 ASP.NET Core 的 `[Authorize]` 特性控制命令的访问权限，可以标记在方法或整个控制器类上。

### 内置角色

框架根据群消息中的发送者身份自动解析以下内置角色：

- `Builtin:GroupOwner` — 群主（同时拥有 GroupAdmin 和 GroupMember）
- `Builtin:GroupAdmin` — 群管理员（同时拥有 GroupMember）
- `Builtin:GroupMember` — 群成员

```csharp
using Microsoft.AspNetCore.Authorization;
using NapArona.Controllers.Authorization;

[Command("/shutup")]
[GroupOnly]
[Authorize(Roles = BuiltinRoles.GroupAdmin)]
public async Task ShutUp()
{
    await ReplyTextAsync("已开启全体禁言");
}
```

单个 `[Authorize]` 内的多个 Roles 是 OR 关系（满足其一即可），多个 `[Authorize]` 之间是 AND 关系（都要满足），与 ASP.NET Core 行为一致。

### 自定义角色

实现 `IAuthorizationProvider` 接口并通过 DI 注册，即可提供自定义角色：

```csharp
public class MyAuthProvider : IAuthorizationProvider
{
    private static readonly HashSet<long> SuperUsers = [123456789, 987654321];

    public Task<IReadOnlySet<string>> GetRolesAsync(BotContext context)
    {
        var roles = new HashSet<string>(StringComparer.Ordinal);
        if (SuperUsers.Contains(context.UserId))
            roles.Add("SuperUser");
        return Task.FromResult<IReadOnlySet<string>>(roles);
    }
}
```

```csharp
builder.Services.AddSingleton<IAuthorizationProvider, MyAuthProvider>();
```

然后就可以在命令上使用自定义角色：

```csharp
[Command("/sudo")]
[Authorize(Roles = "SuperUser")]
public async Task Sudo(params string[] args)
{
    await ReplyTextAsync($"[sudo] 已执行: {string.Join(' ', args)}");
}
```

自定义角色和内置角色可以混用。授权失败时默认静默忽略，也可以在 `IAuthorizationProvider` 中重写 `OnUnauthorizedAsync` 自定义失败行为。

## 上下文

每个命令处理方法执行时，`Context` 属性提供当前消息的完整上下文：

- `Context.SelfId` — 机器人 QQ 号
- `Context.GroupId` — 群号（私聊时为 null）
- `Context.UserId` — 发送者 QQ 号
- `Context.TextContent` — 提取后的纯文本
- `Context.Messages` — 原始消息链
- `Context.Bot` — 用于主动调用 API（发消息、撤回等）

## 事件总线

如果不想用控制器模式，也可以直接订阅 `NapAronaEventBus` 上的事件：

```csharp
var eventBus = app.Services.GetRequiredService<NapAronaEventBus>();
eventBus.OnGroupMessage += botEvent =>
{
    logger.LogInformation("群 {GroupId} 收到消息", botEvent.Event.GroupId);
};
```

事件总线覆盖了 OneBot 协议中的所有事件类型，包括消息、通知、元事件等。

## 多 Bot 支持

框架通过 `BotSessionManager` 管理多个 Bot 的连接。多个 NapCat 实例可以同时连接到同一个 WebSocket 端点，每个连接通过 lifecycle 事件自动识别身份，事件总线会携带 `SelfId` 区分来源。

## 项目结构

- `NapArona.Hosting` — ASP.NET Core 集成层：WebSocket 端点、会话管理、事件总线
- `NapArona.Controllers` — 控制器框架：路由表、命令解析、参数绑定、特性定义
- `NapArona.Example` — 示例项目

依赖 .NET 9 和 NapPlana.NET。
