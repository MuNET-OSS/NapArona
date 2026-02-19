using NapPlana.Core.Bot.BotInstance;
using NapPlana.Core.Data;
using NapPlana.Core.Data.API;
using NapPlana.Core.Data.Action;
using NapPlana.Core.API;
using NapPlana.Core.Event.Handler;
using NapArona.Hosting.Connections;

namespace NapArona.Hosting.Bot;

/// <summary>
/// NapArona Bot上下文 - 反向WebSocket场景下的INapBot实现
/// </summary>
/// <remarks>
/// 负责API调用，通过echo机制实现请求-响应匹配。
/// 不依赖IOptions，生命周期由外部管理。
/// </remarks>
public class NapAronaBotContext : INapBot, IDisposable
{
    private readonly IApiHandler _apiHandler;
    public IEventHandler EventHandler { get; }
    private readonly ReverseWebSocketConnection _connection;
    private bool _disposed;

    /// <summary>
    /// 机器人QQ号
    /// </summary>
    public long SelfId { get; set; }

    /// <summary>
    /// 构造函数 - 通过依赖注入获取核心服务
    /// </summary>
    /// <param name="apiHandler">API处理器</param>
    /// <param name="eventHandler">事件处理器</param>
    /// <param name="connection">反向WebSocket连接实例</param>
    public NapAronaBotContext(
        IApiHandler apiHandler,
        IEventHandler eventHandler,
        ReverseWebSocketConnection connection)
    {
        _apiHandler = apiHandler ?? throw new ArgumentNullException(nameof(apiHandler));
        EventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// 发送消息的统一处理方法
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="message">消息体</param>
    /// <param name="actionType">API操作类型</param>
    /// <param name="timeoutSeconds">超时秒数</param>
    /// <returns>反序列化后的响应数据</returns>
    private async Task<T?> SendMessageAsync<T>(object message, ApiActionType actionType, int timeoutSeconds = 15)
        where T : ResponseDataBase
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NapAronaBotContext));

        if (message is null)
            throw new ArgumentNullException(nameof(message));

        var echo = Guid.NewGuid().ToString();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var cts = new CancellationTokenSource(timeout);

        var tcs = new TaskCompletionSource<ActionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_apiHandler.TryRegister(echo, tcs))
        {
            throw new InvalidOperationException("Echo register failed.");
        }

        // 超时处理
        var registration = cts.Token.Register(() =>
        {
            if (_apiHandler.TryRemove(echo, out var pending))
            {
                pending?.TrySetException(new TimeoutException($"Timed out waiting for {typeof(T).Name} response."));
            }
        });

        try
        {
            await _connection.SendMessageAsync(actionType, message, echo).ConfigureAwait(false);
            var baseResult = await tcs.Task.ConfigureAwait(false);
            var data = baseResult.GetData<T>();
            return data;
        }
        catch
        {
            _apiHandler.TryRemove(echo, out _);
            throw;
        }
        finally
        {
            registration.Dispose();
            cts.Dispose();
        }
    }

    /// <summary>
    /// 发送群聊消息
    /// </summary>
    /// <param name="groupMessage">群消息结构</param>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <returns>群消息发送响应数据</returns>
    public async Task<GroupMessageSendResponseData> SendGroupMessageAsync(GroupMessageSend groupMessage, int timeoutSeconds = 15)
    {
        if (groupMessage is null)
            throw new ArgumentNullException(nameof(groupMessage));

        var res = await SendMessageAsync<GroupMessageSendResponseData>(groupMessage, ApiActionType.SendGroupMsg, timeoutSeconds);
        return res ?? throw new InvalidOperationException("Failed to send group message: response was null.");
    }

    /// <summary>
    /// 发送私聊消息
    /// </summary>
    /// <param name="privateMessage">私聊消息结构</param>
    /// <param name="timeoutSeconds">超时时间（秒）</param>
    /// <returns>私聊消息发送响应数据</returns>
    public async Task<PrivateMessageSendResponseData> SendPrivateMessageAsync(PrivateMessageSend privateMessage, int timeoutSeconds = 15)
    {
        if (privateMessage is null)
            throw new ArgumentNullException(nameof(privateMessage));

        var res = await SendMessageAsync<PrivateMessageSendResponseData>(privateMessage, ApiActionType.SendPrivateMsg, timeoutSeconds);
        return res ?? throw new InvalidOperationException("Failed to send private message: response was null.");
    }

    /// <summary>
    /// 发送戳一戳
    /// </summary>
    /// <param name="pokeMessage">戳一戳消息结构</param>
    public async Task SendPokeAsync(PokeMessageSend pokeMessage)
    {
        if (pokeMessage is null)
            throw new ArgumentNullException(nameof(pokeMessage));

        await SendMessageAsync<ResponseDataBase>(pokeMessage, ApiActionType.SendPoke);
    }

    /// <summary>
    /// 撤回群消息
    /// </summary>
    /// <param name="deleteGroupMessage">撤回消息结构</param>
    public async Task DeleteGroupMessageAsync(GroupMessageDelete deleteGroupMessage)
    {
        if (deleteGroupMessage is null)
            throw new ArgumentNullException(nameof(deleteGroupMessage));

        await SendMessageAsync<ResponseDataBase>(deleteGroupMessage, ApiActionType.DeleteMsg);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        EventHandler.LogReceived(NapPlana.Core.Data.LogLevel.Debug, "NapAronaBotContext 已释放");
    }
}
