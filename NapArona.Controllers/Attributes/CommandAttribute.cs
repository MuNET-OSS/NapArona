namespace NapArona.Controllers.Attributes;

/// <summary>
/// 标记方法为命令处理器，用于匹配用户输入的命令。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// 命令匹配模式。当 <see cref="IsRegex"/> 为 true 时，此模式为正则表达式。
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// 指示 <see cref="Pattern"/> 是否为正则表达式。
    /// </summary>
    public bool IsRegex { get; set; } = false;

    /// <summary>
    /// 初始化命令特性。
    /// </summary>
    /// <param name="pattern">命令匹配模式。</param>
    public CommandAttribute(string pattern)
    {
        Pattern = pattern;
    }
}
