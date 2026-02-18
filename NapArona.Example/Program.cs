using NapArona.Controllers.Authorization;
using NapArona.Controllers.Extensions;
using NapArona.Hosting.Extensions;
using NapArona.Hosting.Events;
using NapArona.Example;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// 注册 NapArona 核心服务，配置 WebSocket 路径和认证
builder.Services.AddNapArona(opts =>
{
    opts.WebSocketPath = "/onebot/{token}";
    opts.AuthenticateAsync = ctx =>
    {
        var token = ctx.Request.RouteValues["token"]?.ToString();
        return Task.FromResult(token == "your-secret-token");
    };
});

// 注册 NapArona Controllers 服务
builder.Services.AddNapAronaControllers();

// 注册自定义授权提供者
builder.Services.AddSingleton<IAuthorizationProvider, ExampleAuthorizationProvider>();

var app = builder.Build();

// 启用 WebSocket 中间件，必须在 MapNapArona 之前调用
app.UseWebSockets();

// 映射 NapArona WebSocket 端点
app.MapNapArona();

// 启用 NapArona Controllers
app.UseNapAronaControllers();

// 通过 EventBus 直接订阅事件
var eventBus = app.Services.GetRequiredService<NapAronaEventBus>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

eventBus.BotConnected += (selfId, ctx) =>
{
    logger.LogInformation("Bot {SelfId} 已连接", selfId);
};

eventBus.BotDisconnected += selfId =>
{
    logger.LogInformation("Bot {SelfId} 已断开", selfId);
};

eventBus.OnGroupMessage += botEvent =>
{
    logger.LogInformation("收到群消息: {GroupId}", botEvent.Event.GroupId);
};

app.Run();
