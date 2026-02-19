using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using NapArona.Controllers.Attributes;
using NapArona.Controllers.Filters;

namespace NapArona.Controllers.Routing;

public sealed record CommandRoute(
    Type ControllerType,
    MethodInfo Method,
    string Pattern,
    bool IsRegex,
    Regex? CompiledRegex,
    bool GroupOnly,
    bool PrivateOnly,
    ParameterInfo[] Parameters,
    IReadOnlyList<string[]> AuthorizeRoles,
    IReadOnlyList<FilterDescriptor> ControllerFilters,
    IReadOnlyList<FilterDescriptor> MethodFilters);

public sealed record EventRoute(
    Type ControllerType,
    MethodInfo Method,
    Type EventType,
    ParameterInfo[] Parameters,
    IReadOnlyList<string[]> AuthorizeRoles,
    IReadOnlyList<FilterDescriptor> ControllerFilters,
    IReadOnlyList<FilterDescriptor> MethodFilters);

public sealed class ControllerRouteTable
{
    private static readonly Lazy<ControllerRouteTable> LazyInstance = new(() => new ControllerRouteTable());

    private readonly object _syncRoot = new();
    private bool _isBuilt;
    private IReadOnlyList<CommandRoute> _commandRoutes = Array.Empty<CommandRoute>();
    private Dictionary<Type, IReadOnlyList<EventRoute>> _eventRoutes = new();

    private ControllerRouteTable()
    {
    }

    public static ControllerRouteTable Instance => LazyInstance.Value;

    public void Build(Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        lock (_syncRoot)
        {
            if (_isBuilt)
                throw new InvalidOperationException("ControllerRouteTable has already been built.");

            var commandRoutes = new List<CommandRoute>();
            var eventRoutes = new Dictionary<Type, List<EventRoute>>();

            foreach (var controllerType in GetControllerTypes(assemblies))
            {
                var classGroupOnly = controllerType.IsDefined(typeof(GroupOnlyAttribute), inherit: true);
                var classPrivateOnly = controllerType.IsDefined(typeof(PrivateOnlyAttribute), inherit: true);

                var methods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(m => m.MetadataToken);

                foreach (var method in methods)
                {
                    var commandAttributes = method.GetCustomAttributes<CommandAttribute>(inherit: true).ToList();
                    var onEventAttribute = method.GetCustomAttribute<OnEventAttribute>(inherit: true);

                    if (commandAttributes.Count > 0 && onEventAttribute is not null)
                    {
                        throw new InvalidOperationException(
                            $"Method '{controllerType.FullName}.{method.Name}' cannot have both [Command] and [OnEvent].");
                    }

                    if (commandAttributes.Count > 0)
                    {
                        var methodGroupOnly = method.IsDefined(typeof(GroupOnlyAttribute), inherit: true);
                        var methodPrivateOnly = method.IsDefined(typeof(PrivateOnlyAttribute), inherit: true);
                        var authorizeRoles = CollectAuthorizeRoles(controllerType, method);
                        var controllerFilters = CollectFilters(controllerType);
                        var methodFilters = CollectMethodFilters(method);

                        foreach (var commandAttribute in commandAttributes)
                        {
                            Regex? compiledRegex = commandAttribute.IsRegex
                                ? new Regex(commandAttribute.Pattern, RegexOptions.Compiled)
                                : null;

                            commandRoutes.Add(new CommandRoute(
                                controllerType,
                                method,
                                commandAttribute.Pattern,
                                commandAttribute.IsRegex,
                                compiledRegex,
                                classGroupOnly || methodGroupOnly,
                                classPrivateOnly || methodPrivateOnly,
                                method.GetParameters(),
                                authorizeRoles,
                                controllerFilters,
                                methodFilters));
                        }

                        continue;
                    }

                    if (onEventAttribute is null)
                        continue;

                    var route = new EventRoute(
                        controllerType,
                        method,
                        onEventAttribute.EventType,
                        method.GetParameters(),
                        CollectAuthorizeRoles(controllerType, method),
                        CollectFilters(controllerType),
                        CollectMethodFilters(method));

                    if (!eventRoutes.TryGetValue(route.EventType, out var routesForEvent))
                    {
                        routesForEvent = new List<EventRoute>();
                        eventRoutes[route.EventType] = routesForEvent;
                    }

                    routesForEvent.Add(route);
                }
            }

            _commandRoutes = commandRoutes.AsReadOnly();
            _eventRoutes = eventRoutes.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<EventRoute>)kvp.Value.AsReadOnly());
            _isBuilt = true;
        }
    }

    public IReadOnlyList<CommandRoute> GetCommandRoutes()
    {
        return _commandRoutes;
    }

    public IReadOnlyList<EventRoute> GetEventRoutes(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        return _eventRoutes.TryGetValue(eventType, out var routes)
            ? routes
            : Array.Empty<EventRoute>();
    }

    /// <summary>
    /// 获取所有事件路由（用于 DI 注册等场景）。
    /// </summary>
    public IEnumerable<EventRoute> GetAllEventRoutes()
    {
        return _eventRoutes.Values.SelectMany(r => r);
    }

    internal static IEnumerable<Type> GetControllerTypes(Assembly[] assemblies)
    {
        return assemblies
            .Where(a => a is not null)
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && typeof(BotController).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal);
    }

    /// <summary>
    /// 收集类和方法上的 [Authorize] 特性，合并为授权角色列表。
    /// 外层 list 各元素之间是 AND 关系，内层 array 各元素之间是 OR 关系。
    /// </summary>
    private static IReadOnlyList<string[]> CollectAuthorizeRoles(Type controllerType, MethodInfo method)
    {
        var classAttrs = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);
        var methodAttrs = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true);
        var allAttrs = classAttrs.Concat(methodAttrs);

        var result = new List<string[]>();
        foreach (var attr in allAttrs)
        {
            var roles = string.IsNullOrWhiteSpace(attr.Roles)
                ? Array.Empty<string>()
                : attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            result.Add(roles);
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// 收集类上的 [BotFilter] 特性，转换为 FilterDescriptor 列表。
    /// </summary>
    private static IReadOnlyList<FilterDescriptor> CollectFilters(Type controllerType)
    {
        return controllerType
            .GetCustomAttributes<BotFilterAttribute>(inherit: true)
            .Select(attr => new FilterDescriptor(attr.FilterType, attr.Order))
            .OrderBy(d => d.Order)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 收集方法上的 [BotFilter] 特性，转换为 FilterDescriptor 列表。
    /// </summary>
    private static IReadOnlyList<FilterDescriptor> CollectMethodFilters(MethodInfo method)
    {
        return method
            .GetCustomAttributes<BotFilterAttribute>(inherit: true)
            .Select(attr => new FilterDescriptor(attr.FilterType, attr.Order))
            .OrderBy(d => d.Order)
            .ToList()
            .AsReadOnly();
    }
}
