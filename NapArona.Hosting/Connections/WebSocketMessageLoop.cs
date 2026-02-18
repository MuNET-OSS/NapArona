using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using NapArona.Hosting.Sessions;
using NapPlana.Core.Data;
using NapPlana.Core.Data.Action;

namespace NapArona.Hosting.Connections;

/// <summary>
/// WebSocket 消息接收循环，负责从 WebSocket 读取帧、拼装完整消息，
/// 并根据消息内容路由到 <see cref="NapPlana.Core.API.ApiHandler"/> 或
/// <see cref="NapPlana.Core.Event.Parser.RootEventParser"/>。
/// </summary>
public static class WebSocketMessageLoop
{
    /// <summary>
    /// 默认接收缓冲区大小（4KB）
    /// </summary>
    private const int BufferSize = 4096;

    /// <summary>
    /// 运行 WebSocket 消息接收循环。
    /// 持续从 WebSocket 读取消息帧，拼装完整消息后进行路由分发：
    /// <list type="bullet">
    ///   <item>含 <c>retcode</c> 字段 → API 响应 → <see cref="NapPlana.Core.API.ApiHandler.Dispatch"/></item>
    ///   <item>其他 → 事件 → <see cref="NapPlana.Core.Event.Parser.RootEventParser.ParseEvent"/></item>
    ///   <item>lifecycle 事件且会话未识别 → 提取 <c>self_id</c> → <see cref="BotSessionManager.IdentifySessionAsync"/></item>
    /// </list>
    /// 当 WebSocket 关闭或取消令牌触发时，自动调用 <see cref="BotSessionManager.RemoveSessionAsync"/>。
    /// </summary>
    /// <param name="session">当前 Bot 会话</param>
    /// <param name="sessionManager">会话管理器</param>
    /// <param name="ct">取消令牌</param>
    public static async Task RunReceiveLoopAsync(
        BotSession session,
        BotSessionManager sessionManager,
        CancellationToken ct)
    {
        var ws = session.Connection.WebSocket;
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        try
        {
            using var messageBuffer = new MemoryStream();

            while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                messageBuffer.SetLength(0);

                // 读取帧，拼装完整消息
                do
                {
                    result = await ws.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // 收到关闭帧，移除会话并退出
                        await sessionManager.RemoveSessionAsync(session);
                        return;
                    }

                    messageBuffer.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                // 解码完整消息
                var text = Encoding.UTF8.GetString(
                    messageBuffer.GetBuffer(), 0, (int)messageBuffer.Length);

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // 每条消息独立 try-catch，避免单条异常中断整个循环
                try
                {
                    await ProcessMessageAsync(text, session, sessionManager);
                }
                catch (Exception ex)
                {
                    session.EventHandler.LogReceived(
                        LogLevel.Debug,
                        $"处理消息时出现异常: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消，不做处理
        }
        catch (WebSocketException)
        {
            // WebSocket 异常断开
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);

            // 确保会话被清理（若尚未移除）
            if (ws.State != WebSocketState.Open)
            {
                await sessionManager.RemoveSessionAsync(session);
            }
        }
    }

    /// <summary>
    /// 处理单条完整消息：路由到 API 响应处理或事件解析，
    /// 并在需要时执行会话身份识别。
    /// </summary>
    /// <param name="text">完整的 JSON 消息文本</param>
    /// <param name="session">当前 Bot 会话</param>
    /// <param name="sessionManager">会话管理器</param>
    private static async Task ProcessMessageAsync(
        string text,
        BotSession session,
        BotSessionManager sessionManager)
    {
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;

        // 含 retcode → API 响应
        if (root.TryGetProperty("retcode", out _))
        {
            var response = JsonSerializer.Deserialize<ActionResponse>(text);
            if (response != null)
            {
                session.ApiHandler.Dispatch(response);
            }

            return;
        }

        // 其他消息 → 事件
        session.EventParser.ParseEvent(text);

        // 若会话尚未识别，尝试从 lifecycle 事件中提取 self_id
        if (!session.IsIdentified
            && root.TryGetProperty("meta_event_type", out var metaType)
            && metaType.GetString() == "lifecycle"
            && root.TryGetProperty("self_id", out var selfIdElement)
            && selfIdElement.TryGetInt64(out var selfId))
        {
            await sessionManager.IdentifySessionAsync(session, selfId);
        }
    }
}
