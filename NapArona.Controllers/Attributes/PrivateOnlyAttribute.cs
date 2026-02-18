namespace NapArona.Controllers.Attributes;

/// <summary>
/// 标记命令处理器仅在私聊中可用。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PrivateOnlyAttribute : Attribute
{
}
