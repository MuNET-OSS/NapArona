using NapArona.Controllers.Filters;

namespace NapArona.Controllers.Attributes;

/// <summary>
/// 标记控制器或方法使用指定的 <see cref="IBotFilter"/>。
/// 可多次标记以应用多个 Filter。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class BotFilterAttribute : Attribute
{
    /// <summary>
    /// Filter 的实现类型。
    /// </summary>
    public Type FilterType { get; }

    /// <summary>
    /// 排序权重，值越小越先执行，默认为 0。
    /// </summary>
    public int Order { get; set; }

    public BotFilterAttribute(Type filterType)
    {
        if (!typeof(IBotFilter).IsAssignableFrom(filterType))
            throw new ArgumentException(
                $"Type '{filterType.FullName}' does not implement {nameof(IBotFilter)}.",
                nameof(filterType));

        FilterType = filterType;
    }
}

/// <summary>
/// 标记控制器或方法使用指定的 <see cref="IBotFilter"/>（泛型版本）。
/// </summary>
/// <typeparam name="TFilter">Filter 类型，必须实现 <see cref="IBotFilter"/>。</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class BotFilterAttribute<TFilter> : BotFilterAttribute where TFilter : IBotFilter
{
    public BotFilterAttribute() : base(typeof(TFilter))
    {
    }
}
