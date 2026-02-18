using Microsoft.Extensions.DependencyInjection;
using NapArona.Hosting.Events;
using NapArona.Hosting.Sessions;

namespace NapArona.Hosting.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> 的扩展方法，用于注册 NapArona 相关服务。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 NapArona 所需的核心服务，包括配置选项、事件总线和会话管理器。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configure">可选的配置回调，用于自定义 <see cref="NapAronaOptions"/>。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddNapArona(
        this IServiceCollection services,
        Action<NapAronaOptions>? configure = null)
    {
        // 注册配置选项
        var options = new NapAronaOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // 注册事件总线
        services.AddSingleton<NapAronaEventBus>();

        // 注册会话管理器
        services.AddSingleton<BotSessionManager>();

        return services;
    }
}
