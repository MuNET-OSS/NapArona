using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NapArona.Controllers.Routing;

namespace NapArona.Controllers.Extensions;

/// <summary>
/// <see cref="IServiceCollection"/> 的扩展方法，用于注册 NapArona Controllers 相关服务。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 NapArona Controllers 所需的服务，包括 BotContext、ControllerRouteTable、BotDispatcher 和控制器类型。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="assemblies">要扫描 BotController 子程序集的集合。若未指定，则使用调用方所在程序集。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddNapAronaControllers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        services.AddScoped<BotContext>();
        services.AddSingleton(_ => ControllerRouteTable.Instance);
        services.AddSingleton<BotDispatcher>();

        foreach (var controllerType in ControllerRouteTable.GetControllerTypes(assemblies))
        {
            services.AddTransient(controllerType);
        }

        ControllerRouteTable.Instance.Build(assemblies);

        return services;
    }
}
