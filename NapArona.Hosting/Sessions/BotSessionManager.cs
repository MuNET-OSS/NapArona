using System.Collections.Concurrent;
using System.Net.WebSockets;
using NapArona.Hosting.Connections;
using NapArona.Hosting.Events;

namespace NapArona.Hosting.Sessions;

/// <summary>
/// 管理所有活跃的 <see cref="BotSession"/>，提供会话的创建、身份识别、移除和查询功能。
/// 使用 <see cref="ConcurrentDictionary{TKey,TValue}"/> 保证线程安全。
/// </summary>
public class BotSessionManager
{
    /// <summary>
    /// 连接级会话存储，key 为连接标识（WebSocket HashCode 或 GUID），
    /// 在身份识别之前用于追踪会话。
    /// </summary>
    private readonly ConcurrentDictionary<string, BotSession> _connectionSessions = new();

    /// <summary>
    /// 已识别会话存储，key 为机器人 QQ 号（selfId），
    /// 在收到 lifecycle 事件完成身份识别后填充。
    /// </summary>
    private readonly ConcurrentDictionary<long, BotSession> _identifiedSessions = new();

    private readonly NapAronaEventBus _eventBus;

    /// <summary>
    /// 初始化 <see cref="BotSessionManager"/>。
    /// </summary>
    /// <param name="eventBus">全局事件总线，用于订阅/取消订阅会话事件。</param>
    public BotSessionManager(NapAronaEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// 为新接入的 WebSocket 连接创建 <see cref="BotSession"/>。
    /// </summary>
    /// <param name="webSocket">ASP.NET Core 接受的 WebSocket 实例。</param>
    /// <returns>新创建的 <see cref="BotSession"/>。</returns>
    public BotSession CreateSession(WebSocket webSocket)
    {
        ArgumentNullException.ThrowIfNull(webSocket);

        var connection = new ReverseWebSocketConnection(webSocket);
        var session = new BotSession(connection);

        _connectionSessions.TryAdd(session.ConnectionId, session);
        return session;
    }

    /// <summary>
    /// 当收到 lifecycle 事件后，将会话与机器人 QQ 号绑定。
    /// 若已存在相同 selfId 的旧会话，则替换并释放旧会话。
    /// </summary>
    /// <param name="session">待识别的会话。</param>
    /// <param name="selfId">机器人 QQ 号。</param>
    public async Task IdentifySessionAsync(BotSession session, long selfId)
    {
        ArgumentNullException.ThrowIfNull(session);

        // 检查是否存在相同 selfId 的旧会话，若有则替换
        if (_identifiedSessions.TryRemove(selfId, out var oldSession) && oldSession != session)
        {
            // 从连接级字典中也移除旧会话
            _connectionSessions.TryRemove(oldSession.ConnectionId, out _);

            // 取消旧会话的事件订阅并释放资源
            _eventBus.UnsubscribeFrom(oldSession);
            await oldSession.DisposeAsync();
        }

        // 设置身份标识
        session.SelfId = selfId;

        // 添加到已识别会话字典
        _identifiedSessions[selfId] = session;

        // 订阅事件并触发连接通知
        _eventBus.SubscribeTo(session);
    }

    /// <summary>
    /// 移除并清理指定会话（连接断开时调用）。
    /// </summary>
    /// <param name="session">要移除的会话。</param>
    public async Task RemoveSessionAsync(BotSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        // 从连接级字典移除
        _connectionSessions.TryRemove(session.ConnectionId, out _);

        // 若已识别，从已识别字典移除并取消事件订阅
        if (session.IsIdentified)
        {
            _identifiedSessions.TryRemove(session.SelfId!.Value, out _);
            _eventBus.UnsubscribeFrom(session);
        }

        // 释放会话资源
        await session.DisposeAsync();
    }

    /// <summary>
    /// 根据机器人 QQ 号查找已识别的会话。
    /// </summary>
    /// <param name="selfId">机器人 QQ 号。</param>
    /// <returns>找到的 <see cref="BotSession"/>，未找到则返回 null。</returns>
    public BotSession? GetSession(long selfId)
    {
        _identifiedSessions.TryGetValue(selfId, out var session);
        return session;
    }

    /// <summary>
    /// 获取所有已识别的活跃会话。
    /// </summary>
    /// <returns>所有已识别会话的集合。</returns>
    public ICollection<BotSession> GetAllSessions()
    {
        return _identifiedSessions.Values;
    }
}
