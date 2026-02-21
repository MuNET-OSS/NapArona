using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NapArona.Controllers.Attributes;
using NapArona.Controllers.Authorization;
using NapArona.Controllers.Filters;
using NapArona.Hosting.Events;
using NapArona.Hosting.Sessions;
using NapPlana.Core.Data;
using NapPlana.Core.Data.Event;
using NapPlana.Core.Data.Event.Message;
using NapPlana.Core.Data.Event.Notice;
using NapPlana.Core.Data.Message;

namespace NapArona.Controllers.Routing;

public sealed class BotDispatcher
{
    private readonly NapAronaEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ControllerRouteTable _routeTable;
    private readonly ILogger<BotDispatcher> _logger;
    private readonly BotSessionManager _sessionManager;
    private readonly IReadOnlyList<FilterDescriptor> _globalFilters;
    private readonly object _syncRoot = new();
    private readonly List<Action> _unsubscribeActions = new();
    private readonly IReadOnlyList<CommandRoute> _commandRoutes;
    private bool _started;

    public BotDispatcher(
        NapAronaEventBus eventBus,
        IServiceProvider serviceProvider,
        ControllerRouteTable routeTable,
        ILogger<BotDispatcher> logger,
        BotSessionManager sessionManager,
        NapAronaControllerOptions options)
    {
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
        _routeTable = routeTable;
        _logger = logger;
        _sessionManager = sessionManager;
        _globalFilters = options.Filters.OrderBy(d => d.Order).ToList().AsReadOnly();
        _commandRoutes = _routeTable.GetCommandRoutes();
    }

    public void Start()
    {
        lock (_syncRoot)
        {
            if (_started)
            {
                return;
            }

            RegisterSubscription<GroupMessageEvent>(
                h => _eventBus.OnGroupMessage += h,
                h => _eventBus.OnGroupMessage -= h,
                HandleGroupMessage);

            RegisterSubscription<PrivateMessageEvent>(
                h => _eventBus.OnPrivateMessage += h,
                h => _eventBus.OnPrivateMessage -= h,
                HandlePrivateMessage);

            RegisterNoticeSubscription<FriendAddNoticeEvent>(
                h => _eventBus.OnFriendAddNotice += h,
                h => _eventBus.OnFriendAddNotice -= h);
            RegisterNoticeSubscription<FriendRecallNoticeEvent>(
                h => _eventBus.OnFriendRecallNotice += h,
                h => _eventBus.OnFriendRecallNotice -= h);
            RegisterNoticeSubscription<GroupAdminNoticeEvent>(
                h => _eventBus.OnGroupAdminNotice += h,
                h => _eventBus.OnGroupAdminNotice -= h);
            RegisterNoticeSubscription<GroupAdminNoticeEvent>(
                h => _eventBus.OnGroupAdminSetNotice += h,
                h => _eventBus.OnGroupAdminSetNotice -= h);
            RegisterNoticeSubscription<GroupAdminNoticeEvent>(
                h => _eventBus.OnGroupAdminUnsetNotice += h,
                h => _eventBus.OnGroupAdminUnsetNotice -= h);
            RegisterNoticeSubscription<GroupBanNoticeEvent>(
                h => _eventBus.OnGroupBanNotice += h,
                h => _eventBus.OnGroupBanNotice -= h);
            RegisterNoticeSubscription<GroupBanNoticeEvent>(
                h => _eventBus.OnGroupBanSetNotice += h,
                h => _eventBus.OnGroupBanSetNotice -= h);
            RegisterNoticeSubscription<GroupBanNoticeEvent>(
                h => _eventBus.OnGroupBanLiftNotice += h,
                h => _eventBus.OnGroupBanLiftNotice -= h);
            RegisterNoticeSubscription<GroupCardEvent>(
                h => _eventBus.OnGroupCardNotice += h,
                h => _eventBus.OnGroupCardNotice -= h);
            RegisterNoticeSubscription<GroupDecreaseNoticeEvent>(
                h => _eventBus.OnGroupDecreaseNotice += h,
                h => _eventBus.OnGroupDecreaseNotice -= h);
            RegisterNoticeSubscription<GroupDecreaseNoticeEvent>(
                h => _eventBus.OnGroupDecreaseLeaveNotice += h,
                h => _eventBus.OnGroupDecreaseLeaveNotice -= h);
            RegisterNoticeSubscription<GroupDecreaseNoticeEvent>(
                h => _eventBus.OnGroupDecreaseKickNotice += h,
                h => _eventBus.OnGroupDecreaseKickNotice -= h);
            RegisterNoticeSubscription<GroupDecreaseNoticeEvent>(
                h => _eventBus.OnGroupDecreaseKickMeNotice += h,
                h => _eventBus.OnGroupDecreaseKickMeNotice -= h);
            RegisterNoticeSubscription<GroupIncreaseNoticeEvent>(
                h => _eventBus.OnGroupIncreaseNotice += h,
                h => _eventBus.OnGroupIncreaseNotice -= h);
            RegisterNoticeSubscription<GroupIncreaseNoticeEvent>(
                h => _eventBus.OnGroupIncreaseApproveNotice += h,
                h => _eventBus.OnGroupIncreaseApproveNotice -= h);
            RegisterNoticeSubscription<GroupIncreaseNoticeEvent>(
                h => _eventBus.OnGroupIncreaseInviteNotice += h,
                h => _eventBus.OnGroupIncreaseInviteNotice -= h);
            RegisterNoticeSubscription<GroupRecallNoticeEvent>(
                h => _eventBus.OnGroupRecallNotice += h,
                h => _eventBus.OnGroupRecallNotice -= h);
            RegisterNoticeSubscription<GroupUploadNoticeEvent>(
                h => _eventBus.OnGroupUploadNotice += h,
                h => _eventBus.OnGroupUploadNotice -= h);
            RegisterNoticeSubscription<GroupEssenceNoticeEvent>(
                h => _eventBus.OnGroupEssenceNotice += h,
                h => _eventBus.OnGroupEssenceNotice -= h);
            RegisterNoticeSubscription<GroupEssenceNoticeEvent>(
                h => _eventBus.OnGroupEssenceAddNotice += h,
                h => _eventBus.OnGroupEssenceAddNotice -= h);
            RegisterNoticeSubscription<GroupEssenceNoticeEvent>(
                h => _eventBus.OnGroupEssenceDeleteNotice += h,
                h => _eventBus.OnGroupEssenceDeleteNotice -= h);
            RegisterNoticeSubscription<GroupMsgEmojiLikeNoticeEvent>(
                h => _eventBus.OnGroupMsgEmojiLikeNotice += h,
                h => _eventBus.OnGroupMsgEmojiLikeNotice -= h);
            RegisterNoticeSubscription<FriendPokeNoticeEvent>(
                h => _eventBus.OnFriendPokeNotice += h,
                h => _eventBus.OnFriendPokeNotice -= h);
            RegisterNoticeSubscription<GroupPokeNoticeEvent>(
                h => _eventBus.OnGroupPoke += h,
                h => _eventBus.OnGroupPoke -= h);
            RegisterNoticeSubscription<InputStatusNoticeEvent>(
                h => _eventBus.OnInputStatusNotice += h,
                h => _eventBus.OnInputStatusNotice -= h);
            RegisterNoticeSubscription<GroupTitleEvent>(
                h => _eventBus.OnGroupTitleNotice += h,
                h => _eventBus.OnGroupTitleNotice -= h);
            RegisterNoticeSubscription<ProfileLikeNoticeEvent>(
                h => _eventBus.OnProfileLikeNotice += h,
                h => _eventBus.OnProfileLikeNotice -= h);

            _started = true;
        }
    }

    public void Stop()
    {
        lock (_syncRoot)
        {
            if (!_started)
            {
                return;
            }

            foreach (var unsubscribe in _unsubscribeActions)
            {
                unsubscribe();
            }

            _unsubscribeActions.Clear();
            _started = false;
        }
    }

    private void HandleGroupMessage(BotEvent<GroupMessageEvent> botEvent)
    {
        // 如果消息以 @某人 开头且不是 @自己，则忽略
        if (botEvent.Event.Messages is { Count: > 0 }
            && botEvent.Event.Messages[0] is { MessageType: MessageDataType.At, MessageData: AtMessageData atData }
            && atData.Qq != botEvent.SelfId.ToString())
        {
            return;
        }

        var text = MessageTextExtractor.ExtractText(botEvent.Event.Messages);

        foreach (var route in _commandRoutes)
        {
            if (!TryMatchCommand(route, text, isGroupMessage: true, out var slashResult, out var regexResult))
            {
                continue;
            }

            _ = Task.Run(async () =>
            {
                await DispatchCommandAsync(
                    route,
                    botEvent,
                    text,
                    botEvent.Event.GroupId,
                    botEvent.Event.UserId,
                    botEvent.Event.Messages,
                    slashResult,
                    regexResult).ConfigureAwait(false);
            });
            return;
        }
    }

    private void HandlePrivateMessage(BotEvent<PrivateMessageEvent> botEvent)
    {
        var text = MessageTextExtractor.ExtractText(botEvent.Event.Messages);

        foreach (var route in _commandRoutes)
        {
            if (!TryMatchCommand(route, text, isGroupMessage: false, out var slashResult, out var regexResult))
            {
                continue;
            }

            _ = Task.Run(async () =>
            {
                await DispatchCommandAsync(
                    route,
                    botEvent,
                    text,
                    null,
                    botEvent.Event.UserId,
                    botEvent.Event.Messages,
                    slashResult,
                    regexResult).ConfigureAwait(false);
            });
            return;
        }
    }

    private void HandleNoticeEvent<TEvent>(BotEvent<TEvent> botEvent)
        where TEvent : OneBotEvent
    {
        var routes = _routeTable.GetEventRoutes(typeof(TEvent));
        if (routes.Count == 0)
        {
            return;
        }

        foreach (var route in routes)
        {
            _ = Task.Run(async () =>
            {
                await DispatchEventAsync(route, botEvent).ConfigureAwait(false);
            });
        }
    }

    private async Task DispatchCommandAsync<TEvent>(
        CommandRoute route,
        BotEvent<TEvent> botEvent,
        string text,
        long? groupId,
        long userId,
        List<MessageBase>? messages,
        CommandParseResult? slashResult,
        RegexParseResult? regexResult)
        where TEvent : OneBotEvent
    {
        IServiceScope? scope = null;

        try
        {
            scope = _serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            var scopedContext = scopedProvider.GetRequiredService<BotContext>();

            FillBotContext(scopedContext, botEvent, text, messages, groupId, userId, _sessionManager);

            var controller = ResolveController(scopedProvider, route.ControllerType);
            if (controller is null)
            {
                return;
            }

            controller.Context = scopedContext;

            // Filter 管道：Global → Controller → Method
            var filterContext = new BotFilterContext
            {
                BotContext = scopedContext,
                ControllerType = route.ControllerType,
                Method = route.Method
            };

            if (!await RunFiltersAsync(
                    _globalFilters, route.ControllerFilters, route.MethodFilters,
                    filterContext, scopedProvider).ConfigureAwait(false))
            {
                _logger.LogDebug(
                    "Filter rejected command route {Controller}.{Method}.",
                    route.ControllerType.FullName,
                    route.Method.Name);
                return;
            }

            // 授权检查
            if (route.AuthorizeRoles.Count > 0)
            {
                var authorized = await AuthorizationChecker.CheckAsync(
                    scopedContext, route.AuthorizeRoles, scopedProvider).ConfigureAwait(false);
                if (!authorized)
                {
                    _logger.LogDebug(
                        "Authorization failed for command route {Controller}.{Method}, user {UserId}.",
                        route.ControllerType.FullName,
                        route.Method.Name,
                        scopedContext.UserId);
                    return;
                }
            }

            if (!TryBindCommandArguments(
                    route,
                    slashResult,
                    regexResult,
                    scopedContext,
                    botEvent.Event,
                    out var args))
            {
                _logger.LogWarning(
                    "Parameter binding failed for command route {Controller}.{Method}.",
                    route.ControllerType.FullName,
                    route.Method.Name);
                return;
            }

            var result = route.Method.Invoke(controller, args);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while dispatching command route {Controller}.{Method}.",
                route.ControllerType.FullName,
                route.Method.Name);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private async Task DispatchEventAsync<TEvent>(EventRoute route, BotEvent<TEvent> botEvent)
        where TEvent : OneBotEvent
    {
        IServiceScope? scope = null;

        try
        {
            scope = _serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;
            var scopedContext = scopedProvider.GetRequiredService<BotContext>();

            FillBotContext(
                scopedContext,
                botEvent,
                string.Empty,
                null,
                TryReadLong(botEvent.Event, "GroupId"),
                TryReadLong(botEvent.Event, "UserId") ?? 0,
                _sessionManager);

            var controller = ResolveController(scopedProvider, route.ControllerType);
            if (controller is null)
            {
                return;
            }

            controller.Context = scopedContext;

            // Filter 管道：Global → Controller → Method
            var filterContext = new BotFilterContext
            {
                BotContext = scopedContext,
                ControllerType = route.ControllerType,
                Method = route.Method
            };

            if (!await RunFiltersAsync(
                    _globalFilters, route.ControllerFilters, route.MethodFilters,
                    filterContext, scopedProvider).ConfigureAwait(false))
            {
                _logger.LogDebug(
                    "Filter rejected event route {Controller}.{Method}.",
                    route.ControllerType.FullName,
                    route.Method.Name);
                return;
            }

            // 授权检查
            if (route.AuthorizeRoles.Count > 0)
            {
                var authorized = await AuthorizationChecker.CheckAsync(
                    scopedContext, route.AuthorizeRoles, scopedProvider).ConfigureAwait(false);
                if (!authorized)
                {
                    _logger.LogDebug(
                        "Authorization failed for event route {Controller}.{Method}, user {UserId}.",
                        route.ControllerType.FullName,
                        route.Method.Name,
                        scopedContext.UserId);
                    return;
                }
            }

            if (!TryBindEventArguments(route, scopedContext, botEvent.Event, out var args))
            {
                _logger.LogWarning(
                    "Parameter binding failed for event route {Controller}.{Method}.",
                    route.ControllerType.FullName,
                    route.Method.Name);
                return;
            }

            var result = route.Method.Invoke(controller, args);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while dispatching event route {Controller}.{Method}.",
                route.ControllerType.FullName,
                route.Method.Name);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private static bool TryMatchCommand(
        CommandRoute route,
        string text,
        bool isGroupMessage,
        out CommandParseResult? slashResult,
        out RegexParseResult? regexResult)
    {
        slashResult = null;
        regexResult = null;

        if (isGroupMessage && route.PrivateOnly)
        {
            return false;
        }

        if (!isGroupMessage && route.GroupOnly)
        {
            return false;
        }

        if (route.IsRegex)
        {
            regexResult = CommandParser.TryParseRegexCommand(text, route.CompiledRegex!);
            return regexResult is not null;
        }

        slashResult = CommandParser.TryParseSlashCommand(text, route.Pattern);
        return slashResult is not null;
    }

    private static void FillBotContext<TEvent>(
        BotContext context,
        BotEvent<TEvent> botEvent,
        string text,
        List<MessageBase>? messages,
        long? groupId,
        long userId,
        BotSessionManager sessionManager)
        where TEvent : OneBotEvent
    {
        context.SelfId = botEvent.SelfId;
        context.Bot = botEvent.BotContext;
        context.Event = botEvent.Event;
        context.GroupId = groupId;
        context.UserId = userId;
        context.TextContent = text;
        context.Messages = messages;
        context.MessageId = botEvent.Event is MessageEventBase msgEvent ? msgEvent.MessageId : null;

        var session = sessionManager.GetSession(botEvent.SelfId);
        if (session is not null)
            context.Items = session.Items;
    }

    private bool TryBindCommandArguments(
        CommandRoute route,
        CommandParseResult? slashResult,
        RegexParseResult? regexResult,
        BotContext context,
        OneBotEvent eventObject,
        out object?[] args)
    {
        if (route.IsRegex)
        {
            return TryBindRegexArguments(route.Parameters, regexResult, context, eventObject, out args);
        }

        return TryBindSlashArguments(route.Parameters, slashResult, context, eventObject, out args);
    }

    private static bool TryBindSlashArguments(
        ParameterInfo[] parameters,
        CommandParseResult? result,
        BotContext context,
        OneBotEvent eventObject,
        out object?[] args)
    {
        args = new object?[parameters.Length];

        if (result is null)
        {
            return false;
        }

        var sourceArgs = result.Args;
        var argIndex = 0;

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (TryBindSpecialParameter(parameter, context, eventObject, null, out var specialValue))
            {
                args[i] = specialValue;
                continue;
            }

            // [RawText]: 将剩余原始文本整体绑定到该 string 参数
            if (parameter.ParameterType == typeof(string) &&
                parameter.IsDefined(typeof(RawTextAttribute), false))
            {
                var remaining = result.RawArgs;
                for (var skip = 0; skip < argIndex; skip++)
                {
                    remaining = remaining.TrimStart();
                    var spaceIdx = remaining.IndexOf(' ');
                    remaining = spaceIdx < 0 ? string.Empty : remaining[spaceIdx..];
                }

                args[i] = remaining.TrimStart();
                argIndex = sourceArgs.Length; // 标记所有参数已消费
                continue;
            }

            if (parameter.ParameterType == typeof(string[]) &&
                (i == parameters.Length - 1 || parameter.IsDefined(typeof(ParamArrayAttribute), false)))
            {
                var remaining = sourceArgs.Skip(argIndex).ToArray();
                args[i] = remaining;
                argIndex = sourceArgs.Length;
                continue;
            }

            if (argIndex >= sourceArgs.Length)
            {
                if (parameter.HasDefaultValue)
                {
                    args[i] = parameter.DefaultValue;
                    continue;
                }

                if (IsNullableParameter(parameter))
                {
                    args[i] = null;
                    continue;
                }

                return false;
            }

            if (!TryConvertArgument(sourceArgs[argIndex], parameter.ParameterType, out var value))
            {
                return false;
            }

            args[i] = value;
            argIndex++;
        }

        // 多余的参数静默丢弃
        return true;
    }

    private static bool TryBindRegexArguments(
        ParameterInfo[] parameters,
        RegexParseResult? result,
        BotContext context,
        OneBotEvent eventObject,
        out object?[] args)
    {
        args = new object?[parameters.Length];

        if (result is null)
        {
            return false;
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (TryBindSpecialParameter(parameter, context, eventObject, result.Match, out var specialValue))
            {
                args[i] = specialValue;
                continue;
            }

            if (!TryGetNamedGroup(result.NamedGroups, parameter.Name, out var rawValue))
            {
                if (parameter.HasDefaultValue)
                {
                    args[i] = parameter.DefaultValue;
                    continue;
                }

                if (IsNullableParameter(parameter))
                {
                    args[i] = null;
                    continue;
                }

                return false;
            }

            if (!TryConvertArgument(rawValue, parameter.ParameterType, out var convertedValue))
            {
                return false;
            }

            args[i] = convertedValue;
        }

        return true;
    }

    private static bool TryBindEventArguments(
        EventRoute route,
        BotContext context,
        OneBotEvent eventObject,
        out object?[] args)
    {
        args = new object?[route.Parameters.Length];

        for (var i = 0; i < route.Parameters.Length; i++)
        {
            var parameter = route.Parameters[i];

            if (TryBindSpecialParameter(parameter, context, eventObject, null, out var value))
            {
                args[i] = value;
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                args[i] = parameter.DefaultValue;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool TryBindSpecialParameter(
        ParameterInfo parameter,
        BotContext context,
        OneBotEvent eventObject,
        Match? match,
        out object? value)
    {
        if (parameter.ParameterType == typeof(BotContext))
        {
            value = context;
            return true;
        }

        if (parameter.ParameterType == typeof(Match) && match is not null)
        {
            value = match;
            return true;
        }

        if (parameter.ParameterType.IsInstanceOfType(eventObject))
        {
            value = eventObject;
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryConvertArgument(string raw, Type targetType, out object? value)
    {
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (type == typeof(string))
        {
            value = raw;
            return true;
        }

        if (type == typeof(int) && int.TryParse(raw, out var intValue))
        {
            value = intValue;
            return true;
        }

        if (type == typeof(long) && long.TryParse(raw, out var longValue))
        {
            value = longValue;
            return true;
        }

        if (type == typeof(bool) && bool.TryParse(raw, out var boolValue))
        {
            value = boolValue;
            return true;
        }

        if (type == typeof(double) &&
            (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var invariantDouble) ||
             double.TryParse(raw, out invariantDouble)))
        {
            value = invariantDouble;
            return true;
        }

        value = null;
        return false;
    }

    private static bool IsNullableParameter(ParameterInfo parameter)
    {
        // Nullable<T> (int?, long?, etc.)
        if (Nullable.GetUnderlyingType(parameter.ParameterType) is not null)
            return true;

        // Nullable reference type (string?, etc.)
        if (!parameter.ParameterType.IsValueType)
        {
            var nullableAttr = parameter.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>();
            if (nullableAttr is { NullableFlags.Length: > 0 })
                return nullableAttr.NullableFlags[0] == 2;

            // Check NullableContextAttribute on method/class for default context
            var contextAttr =
                parameter.Member.GetCustomAttribute<System.Runtime.CompilerServices.NullableContextAttribute>()
                ?? parameter.Member.DeclaringType?.GetCustomAttribute<System.Runtime.CompilerServices.NullableContextAttribute>();
            if (contextAttr is not null)
                return contextAttr.Flag == 2;
        }

        return false;
    }

    private BotController? ResolveController(IServiceProvider provider, Type controllerType)
    {
        var controller = provider.GetRequiredService(controllerType);
        if (controller is BotController typedController)
        {
            return typedController;
        }

        _logger.LogError(
            "Resolved controller type {ControllerType} does not inherit BotController.",
            controllerType.FullName);
        return null;
    }

    /// <summary>
    /// 按 Global → Controller → Method 顺序执行 Filter 管道。
    /// 同级内按 Order 排序，任何一个返回 false 即短路。
    /// </summary>
    private async Task<bool> RunFiltersAsync(
        IReadOnlyList<FilterDescriptor> globalFilters,
        IReadOnlyList<FilterDescriptor> controllerFilters,
        IReadOnlyList<FilterDescriptor> methodFilters,
        BotFilterContext context,
        IServiceProvider scopedProvider)
    {
        // Global filters（已在构造时按 Order 排序并冻结）
        foreach (var descriptor in globalFilters)
        {
            if (!await ExecuteFilterAsync(descriptor, context, scopedProvider).ConfigureAwait(false))
                return false;
        }

        // Controller filters（已在 ControllerRouteTable 中按 Order 排序）
        foreach (var descriptor in controllerFilters)
        {
            if (!await ExecuteFilterAsync(descriptor, context, scopedProvider).ConfigureAwait(false))
                return false;
        }

        // Method filters（已在 ControllerRouteTable 中按 Order 排序）
        foreach (var descriptor in methodFilters)
        {
            if (!await ExecuteFilterAsync(descriptor, context, scopedProvider).ConfigureAwait(false))
                return false;
        }

        return true;
    }

    private async Task<bool> ExecuteFilterAsync(
        FilterDescriptor descriptor,
        BotFilterContext context,
        IServiceProvider scopedProvider)
    {
        try
        {
            var filter = (IBotFilter)scopedProvider.GetRequiredService(descriptor.FilterType);
            return await filter.OnExecutingAsync(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Filter {FilterType} threw an exception, treating as rejected.",
                descriptor.FilterType.FullName);
            return false;
        }
    }

    private void RegisterNoticeSubscription<TEvent>(
        Action<Action<BotEvent<TEvent>>> subscribe,
        Action<Action<BotEvent<TEvent>>> unsubscribe)
        where TEvent : OneBotEvent
    {
        Action<BotEvent<TEvent>> handler = HandleNoticeEvent;
        RegisterSubscription(subscribe, unsubscribe, handler);
    }

    private void RegisterSubscription<TEvent>(
        Action<Action<BotEvent<TEvent>>> subscribe,
        Action<Action<BotEvent<TEvent>>> unsubscribe,
        Action<BotEvent<TEvent>> handler)
    {
        subscribe(handler);
        _unsubscribeActions.Add(() => unsubscribe(handler));
    }

    private static bool TryGetNamedGroup(
        IReadOnlyDictionary<string, string> namedGroups,
        string? parameterName,
        out string value)
    {
        if (parameterName is not null && namedGroups.TryGetValue(parameterName, out var exactValue))
        {
            value = exactValue;
            return true;
        }

        if (parameterName is not null)
        {
            foreach (var pair in namedGroups)
            {
                if (string.Equals(pair.Key, parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }
        }

        value = string.Empty;
        return false;
    }

    private static long? TryReadLong(object source, string propertyName)
    {
        var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
        {
            return null;
        }

        var rawValue = property.GetValue(source);
        if (rawValue is null)
        {
            return null;
        }

        return rawValue switch
        {
            long longValue => longValue,
            int intValue => intValue,
            string str when long.TryParse(str, out var parsed) => parsed,
            _ => null
        };
    }
}
