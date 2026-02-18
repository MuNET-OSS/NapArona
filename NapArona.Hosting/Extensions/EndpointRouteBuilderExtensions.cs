using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NapArona.Hosting.Connections;
using NapArona.Hosting.Sessions;

namespace NapArona.Hosting.Extensions;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> 的扩展方法，用于映射 NapArona WebSocket 端点。
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 映射 NapArona 的 WebSocket 端点，处理来自 NapCat 的反向 WebSocket 连接。
    /// </summary>
    /// <param name="endpoints">端点路由构建器。</param>
    /// <param name="path">
    /// WebSocket 监听路径，为 null 时使用 <see cref="NapAronaOptions.WebSocketPath"/> 的值（默认 "/onebot"）。
    /// </param>
    /// <returns>端点约定构建器，可用于进一步配置端点。</returns>
    public static IEndpointConventionBuilder MapNapArona(
        this IEndpointRouteBuilder endpoints,
        string? path = null)
    {
        // 若未指定路径，从选项中获取
        if (path is null)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<NapAronaOptions>();
            path = options.WebSocketPath;
        }

        return endpoints.Map(path + "/{**catchAll}", async context =>
        {
            // 1. 检查是否为 WebSocket 请求
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            // 2. 获取配置选项
            var options = context.RequestServices.GetRequiredService<NapAronaOptions>();

            // 3. 执行认证（如果配置了认证回调）
            if (options.AuthenticateAsync is not null)
            {
                var authenticated = await options.AuthenticateAsync(context);
                if (!authenticated)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // 4. 接受 WebSocket 连接
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            // 5. 创建会话
            var sessionManager = context.RequestServices.GetRequiredService<BotSessionManager>();
            var session = sessionManager.CreateSession(webSocket);

            // 6. 运行消息接收循环（阻塞直到连接断开）
            // 循环内部会处理身份识别、事件订阅和会话清理
            await WebSocketMessageLoop.RunReceiveLoopAsync(
                session, sessionManager, context.RequestAborted);
        });
    }
}
