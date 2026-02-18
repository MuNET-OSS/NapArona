using Microsoft.AspNetCore.Http;

namespace NapArona.Hosting;

/// <summary>
/// NapArona配置选项
/// </summary>
public class NapAronaOptions
{
    /// <summary>
    /// WebSocket路径，支持路由模板参数（如 "/onebot/{token}"）。
    /// </summary>
    /// <remarks>
    /// 路径中的参数可在 <see cref="AuthenticateAsync"/> 回调中通过
    /// <c>HttpContext.Request.RouteValues</c> 获取。
    /// </remarks>
    public string WebSocketPath { get; set; } = "/onebot";

    /// <summary>
    /// 认证回调函数
    /// </summary>
    /// <remarks>
    /// 返回 true 表示认证通过，false 表示认证失败
    /// </remarks>
    public Func<HttpContext, Task<bool>>? AuthenticateAsync { get; set; }
}
