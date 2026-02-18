namespace NapArona.Controllers.Filters;

/// <summary>
/// 全局 Filter 集合，用于在 <see cref="NapAronaControllerOptions"/> 中注册全局过滤器。
/// </summary>
public sealed class FilterCollection : List<FilterDescriptor>
{
    /// <summary>
    /// 添加一个全局 Filter。
    /// </summary>
    /// <typeparam name="TFilter">Filter 类型，必须实现 <see cref="IBotFilter"/>。</typeparam>
    /// <param name="order">排序权重，值越小越先执行，默认为 0。</param>
    public void Add<TFilter>(int order = 0) where TFilter : IBotFilter
    {
        Add(new FilterDescriptor(typeof(TFilter), order));
    }
}
