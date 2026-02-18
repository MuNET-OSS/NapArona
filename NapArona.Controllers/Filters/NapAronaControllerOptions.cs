namespace NapArona.Controllers.Filters;

/// <summary>
/// NapArona Controllers 配置选项。
/// </summary>
public sealed class NapAronaControllerOptions
{
    /// <summary>
    /// 全局 Filter 列表，对所有命令和事件生效。
    /// </summary>
    public FilterCollection Filters { get; } = new();
}
