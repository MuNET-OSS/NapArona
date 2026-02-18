using NapArona.Controllers;
using NapArona.Controllers.Attributes;
using NapArona.Example.Filters;

namespace NapArona.Example.Controllers;

/// <summary>
/// 过滤器演示控制器，展示 GroupOnly/PrivateOnly 和自定义 Filter 的使用
/// </summary>
public class FilterDemoController : BotController
{
    /// <summary>
    /// 群聊专属命令，只能在群聊中使用
    /// </summary>
    [Command("/grouponly")]
    [GroupOnly]
    public async Task GroupOnlyAsync()
    {
        await ReplyTextAsync("这条命令只能在群聊中使用");
    }

    /// <summary>
    /// 私聊专属命令，只能在私聊中使用
    /// </summary>
    [Command("/privateonly")]
    [PrivateOnly]
    public async Task PrivateOnlyAsync()
    {
        await ReplyTextAsync("这条命令只能在私聊中使用");
    }

    /// <summary>
    /// 今日运势 —— 带冷却的方法级 Filter 示例，同一用户 10 秒内只能查一次
    /// </summary>
    [Command("/fortune")]
    [BotFilter<CooldownFilter>]
    public async Task FortuneAsync()
    {
        string[] fortunes = ["大吉", "中吉", "小吉", "吉", "末吉", "小凶", "凶"];
        var today = fortunes[Math.Abs(HashCode.Combine(Context.UserId, DateTime.Today)) % fortunes.Length];
        await ReplyTextAsync($"🔮 你今天的运势是：{today}（10 秒后可再次查看）");
    }
}

/// <summary>
/// 内测功能控制器 —— 整个控制器只在白名单群中可用。
/// 白名单由 GroupWhitelistFilter 控制（Controller 级 Filter 示例）。
/// 适用于将实验性功能限定在特定群内灰度测试的场景。
/// </summary>
[BotFilter<GroupWhitelistFilter>]
[GroupOnly]
public class BetaFeatureController : BotController
{
    [Command("/beta")]
    public async Task BetaInfoAsync()
    {
        await ReplyTextAsync("✨ 你所在的群已开启内测功能！");
    }

    [Command("/feedback")]
    public async Task FeedbackAsync(params string[] words)
    {
        await ReplyTextAsync($"📝 已收到反馈：{string.Join(' ', words)}，感谢参与内测！");
    }
}
