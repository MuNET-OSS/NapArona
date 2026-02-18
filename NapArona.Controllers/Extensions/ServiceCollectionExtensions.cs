using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NapArona.Controllers.Filters;
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

        return services.AddNapAronaControllers(_ => { }, assemblies);
    }

    /// <summary>
    /// 注册 NapArona Controllers 所需的服务，并配置选项（如全局 Filter）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configure">配置回调。</param>
    /// <param name="assemblies">要扫描 BotController 子程序集的集合。若未指定，则使用调用方所在程序集。</param>
    /// <returns>服务集合，支持链式调用。</returns>
    public static IServiceCollection AddNapAronaControllers(
        this IServiceCollection services,
        Action<NapAronaControllerOptions> configure,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var options = new NapAronaControllerOptions();
        configure(options);

        services.AddScoped<BotContext>();
        services.AddSingleton(_ => ControllerRouteTable.Instance);
        services.AddSingleton(options);
        services.AddSingleton<BotDispatcher>();

        foreach (var controllerType in ControllerRouteTable.GetControllerTypes(assemblies))
        {
            services.AddTransient(controllerType);
        }

        ControllerRouteTable.Instance.Build(assemblies);

        // 注册全局 filter 类型
        foreach (var descriptor in options.Filters)
        {
            services.TryAddTransientFilter(descriptor.FilterType);
        }

        // 注册路由中发现的 filter 类型
        foreach (var route in ControllerRouteTable.Instance.GetCommandRoutes())
        {
            RegisterRouteFilters(services, route.ControllerFilters);
            RegisterRouteFilters(services, route.MethodFilters);
        }

        foreach (var route in ControllerRouteTable.Instance.GetAllEventRoutes())
        {
            RegisterRouteFilters(services, route.ControllerFilters);
            RegisterRouteFilters(services, route.MethodFilters);
        }

        return services;
    }

    private static void RegisterRouteFilters(
        IServiceCollection services,
        IReadOnlyList<FilterDescriptor> filters)
    {
        foreach (var descriptor in filters)
        {
            services.TryAddTransientFilter(descriptor.FilterType);
        }
    }

    private static void TryAddTransientFilter(
        this IServiceCollection services,
        Type filterType)
    {
        // 避免重复注册同一类型
        foreach (var existing in services)
        {
            if (existing.ServiceType == filterType)
                return;
        }

        services.AddTransient(filterType);
    }
}
