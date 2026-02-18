namespace NapArona.Controllers.Filters;

/// <summary>
/// 描述一个 Filter 的类型和排序权重。
/// </summary>
public sealed class FilterDescriptor
{
    /// <summary>
    /// Filter 的实现类型，必须实现 <see cref="IBotFilter"/>。
    /// </summary>
    public Type FilterType { get; }

    /// <summary>
    /// 排序权重，值越小越先执行，默认为 0。
    /// </summary>
    public int Order { get; }

    public FilterDescriptor(Type filterType, int order = 0)
    {
        if (!typeof(IBotFilter).IsAssignableFrom(filterType))
            throw new ArgumentException(
                $"Type '{filterType.FullName}' does not implement {nameof(IBotFilter)}.",
                nameof(filterType));

        if (filterType.IsAbstract || filterType.IsInterface)
            throw new ArgumentException(
                $"Type '{filterType.FullName}' cannot be abstract or an interface.",
                nameof(filterType));

        if (filterType.ContainsGenericParameters)
            throw new ArgumentException(
                $"Type '{filterType.FullName}' cannot be an open generic type.",
                nameof(filterType));

        FilterType = filterType;
        Order = order;
    }
}
