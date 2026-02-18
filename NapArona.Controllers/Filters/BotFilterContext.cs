using System.Reflection;

namespace NapArona.Controllers.Filters;

/// <summary>
/// Filter 执行上下文，包含当前命令的路由信息和 Bot 上下文。
/// </summary>
public sealed class BotFilterContext
{
    /// <summary>
    /// 当前 Bot 上下文。
    /// </summary>
    public required BotContext BotContext { get; init; }

    /// <summary>
    /// 匹配到的控制器类型。
    /// </summary>
    public required Type ControllerType { get; init; }

    /// <summary>
    /// 匹配到的方法。
    /// </summary>
    public required MethodInfo Method { get; init; }
}
