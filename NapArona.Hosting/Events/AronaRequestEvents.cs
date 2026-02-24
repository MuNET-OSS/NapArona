using System.Text.Json.Serialization;
using NapArona.Hosting.Bot;
using NapPlana.Core.Data.Event.Request;

namespace NapArona.Hosting.Events;

/// <summary>
/// NapArona 增强的好友请求事件，自动注入 Bot 上下文，支持无参 AcceptAsync/RejectAsync。
/// </summary>
public class AronaFriendRequestEvent : FriendRequestEvent
{
    /// <summary>
    /// 机器人上下文，由 NapArona 框架自动注入。
    /// </summary>
    [JsonIgnore]
    public NapAronaBotContext Bot { get; set; } = null!;

    /// <summary>
    /// 同意好友请求。
    /// </summary>
    /// <param name="remark">同意后的备注名（可选）。</param>
    public Task AcceptAsync(string? remark = null) => AcceptAsync(Bot, remark);

    /// <summary>
    /// 拒绝好友请求。
    /// </summary>
    public Task RejectAsync() => RejectAsync(Bot);
}

/// <summary>
/// NapArona 增强的群请求事件，自动注入 Bot 上下文，支持无参 AcceptAsync/RejectAsync。
/// </summary>
public class AronaGroupRequestEvent : GroupRequestEvent
{
    /// <summary>
    /// 机器人上下文，由 NapArona 框架自动注入。
    /// </summary>
    [JsonIgnore]
    public NapAronaBotContext Bot { get; set; } = null!;

    /// <summary>
    /// 同意群请求。
    /// </summary>
    public Task AcceptAsync() => AcceptAsync(Bot);

    /// <summary>
    /// 拒绝群请求。
    /// </summary>
    /// <param name="reason">拒绝理由（可选）。</param>
    public Task RejectAsync(string? reason = null) => RejectAsync(Bot, reason);
}
