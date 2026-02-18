using NapArona.Controllers;
using NapArona.Controllers.Attributes;
using NapPlana.Core.Data.Event.Notice;

namespace NapArona.Example.Controllers;

/// <summary>
/// 事件演示控制器，展示如何处理各种 OneBot 事件
/// </summary>
public class EventDemoController : BotController
{
    /// <summary>
    /// 群戳一戳事件处理
    /// </summary>
    [OnGroupPoke]
    public async Task OnGroupPokeAsync()
    {
        await ReplyTextAsync("别戳我！");
    }

    /// <summary>
    /// 群成员增加事件处理，发送欢迎消息
    /// </summary>
    [OnGroupIncrease]
    public async Task OnGroupIncreaseAsync(GroupIncreaseNoticeEvent ev)
    {
        await ReplyTextAsync($"欢迎 {ev.UserId} 加入群聊！");
    }

    /// <summary>
    /// 好友戳一戳事件处理，使用 OnEvent 特性指定事件类型
    /// </summary>
    [OnEvent(typeof(FriendPokeNoticeEvent))]
    public async Task OnFriendPokeAsync()
    {
        await ReplyTextAsync("你戳我干嘛～");
    }
}
