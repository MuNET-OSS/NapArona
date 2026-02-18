using NapArona.Hosting.Bot;
using NapPlana.Core.Data.Event;
using NapPlana.Core.Data.Message;

namespace NapArona.Controllers;

/// <summary>
/// Bot 上下文 - Scoped 生命周期，用于在命令处理器中访问当前消息上下文
/// </summary>
public class BotContext
{
    /// <summary>
    /// Bot 上下文，用于发送消息
    /// </summary>
    public NapAronaBotContext Bot { get; internal set; } = null!;

    /// <summary>
    /// 当前机器人的 QQ 号
    /// </summary>
    public long SelfId { get; internal set; }

    /// <summary>
    /// 原始 OneBot 事件
    /// </summary>
    public OneBotEvent Event { get; internal set; } = null!;

    /// <summary>
    /// 群号（如果是群消息则有值，否则为 null）
    /// </summary>
    public long? GroupId { get; internal set; }

    /// <summary>
    /// 发送者 QQ 号
    /// </summary>
    public long UserId { get; internal set; }

    /// <summary>
    /// 提取后的消息文本（非消息事件为空字符串）
    /// </summary>
    public string TextContent { get; internal set; } = string.Empty;

    /// <summary>
    /// 原始消息链（非消息事件为 null）
    /// </summary>
    public List<MessageBase>? Messages { get; internal set; }

    /// <summary>
    /// 连接级自定义数据，来源于 WebSocket 认证阶段通过 <c>HttpContext.Items</c> 写入的内容。
    /// </summary>
    public IDictionary<string, object?> Items { get; internal set; } = new Dictionary<string, object?>();
}
