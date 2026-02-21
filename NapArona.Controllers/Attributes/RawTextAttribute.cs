namespace NapArona.Controllers.Attributes;

/// <summary>
/// 标记在 string 类型的参数上，表示从该参数开始不再按空格拆分，
/// 将剩余的原始文本整体作为该参数的值。
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class RawTextAttribute : Attribute;
