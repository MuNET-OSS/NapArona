namespace NapArona.Controllers.Authorization;

/// <summary>
/// 内置角色常量，框架根据群成员身份自动解析。
/// </summary>
public static class BuiltinRoles
{
    public const string Prefix = "Builtin:";

    /// <summary>
    /// 群主
    /// </summary>
    public const string GroupOwner = "Builtin:GroupOwner";

    /// <summary>
    /// 群管理员（不含群主）
    /// </summary>
    public const string GroupAdmin = "Builtin:GroupAdmin";

    /// <summary>
    /// 群成员（在群里就算，含管理员和群主）
    /// </summary>
    public const string GroupMember = "Builtin:GroupMember";
}
