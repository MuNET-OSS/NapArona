using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NapArona.Controllers.Routing;

namespace NapArona.Controllers.Extensions;

/// <summary>
/// <see cref="IHost"/> 的扩展方法，用于启动 NapArona Controllers。
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 启动 NapArona Controllers 的消息调度器。
    /// </summary>
    /// <param name="host">应用程序主机。</param>
    /// <returns>应用程序主机，支持链式调用。</returns>
    public static IHost UseNapAronaControllers(this IHost host)
    {
        var dispatcher = host.Services.GetRequiredService<BotDispatcher>();
        dispatcher.Start();
        return host;
    }
}
