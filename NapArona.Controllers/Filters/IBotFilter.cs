namespace NapArona.Controllers.Filters;

/// <summary>
/// Bot 过滤器接口。
/// 实现此接口并通过 DI 注册，可在命令或事件执行前进行自定义检查。
/// Filter 从 DI 容器解析，支持构造函数注入。
/// </summary>
public interface IBotFilter
{
    /// <summary>
    /// 在命令或事件执行前调用。返回 true 继续执行，返回 false 中断管道。
    /// </summary>
    /// <param name="context">Filter 上下文。</param>
    /// <returns>true 表示继续执行，false 表示中断。</returns>
    Task<bool> OnExecutingAsync(BotFilterContext context);
}
