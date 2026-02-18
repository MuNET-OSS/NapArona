using NapArona.Hosting.Bot;
using NapArona.Hosting.Connections;
using NapPlana.Core.API;
using NapPlana.Core.Event.Parser;

namespace NapArona.Hosting.Sessions;

/// <summary>
/// 封装单个Bot的完整运行时上下文。
/// 持有独立的EventHandler、ApiHandler、EventParser、Connection和BotContext实例。
/// </summary>
public class BotSession : IAsyncDisposable
{
    /// <summary>
    /// 反向WebSocket连接实例
    /// </summary>
    public ReverseWebSocketConnection Connection { get; }

    /// <summary>
    /// 事件处理器（独立实例，非DI注入）
    /// </summary>
    public NapPlana.Core.Event.Handler.EventHandler EventHandler { get; }

    /// <summary>
    /// API处理器（独立实例，非DI注入）
    /// </summary>
    public ApiHandler ApiHandler { get; }

    /// <summary>
    /// 根事件解析器，负责将原始JSON事件分发到具体子解析器
    /// </summary>
    public RootEventParser EventParser { get; }

    /// <summary>
    /// Bot上下文，提供API调用能力
    /// </summary>
    public NapAronaBotContext Context { get; }

    /// <summary>
    /// 连接唯一标识符，用于在身份识别前追踪会话
    /// </summary>
    public string ConnectionId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 机器人QQ号，在收到lifecycle事件后设置
    /// </summary>
    public long? SelfId { get; set; }

    /// <summary>
    /// 是否已通过lifecycle事件完成身份识别
    /// </summary>
    public bool IsIdentified => SelfId.HasValue;

    /// <summary>
    /// 用于取消消息接收循环的CancellationTokenSource
    /// </summary>
    public CancellationTokenSource Cts { get; }

    /// <summary>
    /// 创建BotSession，初始化所有独立的处理器实例
    /// </summary>
    /// <param name="connection">反向WebSocket连接</param>
    public BotSession(ReverseWebSocketConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        EventHandler = new NapPlana.Core.Event.Handler.EventHandler();
        ApiHandler = new ApiHandler();
        EventParser = new RootEventParser(EventHandler);
        Context = new NapAronaBotContext(ApiHandler, EventHandler, connection);
        Cts = new CancellationTokenSource();
    }

    /// <summary>
    /// 异步释放资源：取消接收循环、关闭连接、清理上下文
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // 取消消息接收循环
        await Cts.CancelAsync();
        Cts.Dispose();

        // 取消所有 pending 的 API 请求
        // ApiHandler.CancelAll();

        // 关闭WebSocket连接
        try
        {
            await Connection.ShutdownAsync();
        }
        catch
        {
            // 连接可能已断开，忽略关闭异常
        }

        // 释放BotContext
        Context.Dispose();
    }
}
