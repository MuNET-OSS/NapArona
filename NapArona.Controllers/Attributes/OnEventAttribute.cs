namespace NapArona.Controllers.Attributes;

/// <summary>
/// 标记方法为事件处理器，用于监听特定类型的事件。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OnEventAttribute : Attribute
{
    /// <summary>
    /// 要监听的事件类型。
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// 初始化事件处理器特性。
    /// </summary>
    /// <param name="eventType">要监听的事件类型。</param>
    public OnEventAttribute(Type eventType)
    {
        EventType = eventType;
    }
}
