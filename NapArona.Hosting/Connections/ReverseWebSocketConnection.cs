using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using NapPlana.Core.API;
using NapPlana.Core.Connections;
using NapPlana.Core.Data;
using NapPlana.Core.Data.Action;
using NapPlana.Core.Data.API;

namespace NapArona.Hosting.Connections;

/// <summary>
/// 反向WebSocket连接，包装ASP.NET Core的WebSocket实例。
/// 由NapCat主动连接到本机WebSocket服务器时使用。
/// </summary>
public class ReverseWebSocketConnection : ConnectionBase
{
    private readonly WebSocket _webSocket;

    /// <summary>
    /// 底层 WebSocket 实例（供消息接收循环使用）
    /// </summary>
    internal WebSocket WebSocket => _webSocket;

    /// <summary>
    /// 初始化反向WebSocket连接
    /// </summary>
    /// <param name="webSocket">ASP.NET Core接受的WebSocket实例</param>
    public ReverseWebSocketConnection(WebSocket webSocket)
    {
        _webSocket = webSocket;
        ConnectionType = BotConnectionType.WebSocketServer;
    }

    /// <summary>
    /// 发送原始消息字符串
    /// </summary>
    /// <param name="message">消息JSON字符串</param>
    public override async Task SendMessageAsync(string message)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None);
    }

    /// <summary>
    /// 构造WsGlobalRequest并发送序列化后的消息
    /// </summary>
    /// <param name="actionType">操作类型</param>
    /// <param name="message">消息内容</param>
    /// <param name="echo">请求标识符</param>
    public override async Task SendMessageAsync(ApiActionType actionType, object message, string echo)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        if (string.IsNullOrEmpty(echo))
        {
            return;
        }

        var req = new WsGlobalRequest
        {
            Action = actionType,
            Params = message,
            Echo = echo
        };

        var json = JsonSerializer.Serialize(req);
        await SendMessageAsync(json);
    }

    /// <summary>
    /// 关闭WebSocket连接
    /// </summary>
    public override async Task ShutdownAsync()
    {
        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Shutdown",
                CancellationToken.None);
        }
    }
}
