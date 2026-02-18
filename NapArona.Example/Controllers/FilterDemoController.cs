using NapArona.Controllers;
using NapArona.Controllers.Attributes;

namespace NapArona.Example.Controllers;

/// <summary>
/// 过滤器演示控制器，展示 GroupOnly 和 PrivateOnly 过滤器的使用
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
}
