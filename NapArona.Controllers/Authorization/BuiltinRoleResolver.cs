using NapPlana.Core.Data;
using NapPlana.Core.Data.Event.Message;

namespace NapArona.Controllers.Authorization;

/// <summary>
/// 解析内置角色 (Builtin:*) —— 根据群消息事件中的 Sender.Role 自动判断。
/// </summary>
internal static class BuiltinRoleResolver
{
    public static HashSet<string> Resolve(BotContext context)
    {
        var roles = new HashSet<string>(StringComparer.Ordinal);

        if (context.Event is not GroupMessageEvent groupEvent)
            return roles;

        // 在群里就是群成员
        roles.Add(BuiltinRoles.GroupMember);

        switch (groupEvent.Sender.Role)
        {
            case GroupRole.Owner:
                roles.Add(BuiltinRoles.GroupOwner);
                roles.Add(BuiltinRoles.GroupAdmin);
                break;
            case GroupRole.Admin:
                roles.Add(BuiltinRoles.GroupAdmin);
                break;
        }

        return roles;
    }
}
