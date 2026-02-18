using Microsoft.Extensions.DependencyInjection;

namespace NapArona.Controllers.Authorization;

/// <summary>
/// 授权检查器，合并内置角色和自定义角色后检查 [Authorize] 要求。
/// </summary>
internal static class AuthorizationChecker
{
    /// <summary>
    /// 检查当前上下文是否满足所有授权要求。
    /// </summary>
    /// <param name="context">Bot 上下文。</param>
    /// <param name="authorizeRoles">路由上预存的授权角色列表（AND of OR）。</param>
    /// <param name="serviceProvider">用于解析 IAuthorizationProvider。</param>
    /// <returns>true 表示授权通过，false 表示拒绝。</returns>
    public static async Task<bool> CheckAsync(
        BotContext context,
        IReadOnlyList<string[]> authorizeRoles,
        IServiceProvider serviceProvider)
    {
        if (authorizeRoles.Count == 0)
            return true;

        // 1. 解析内置角色
        var userRoles = BuiltinRoleResolver.Resolve(context);

        // 2. 合并自定义角色
        var provider = serviceProvider.GetService<IAuthorizationProvider>();
        if (provider is not null)
        {
            var customRoles = await provider.GetRolesAsync(context).ConfigureAwait(false);
            foreach (var role in customRoles)
                userRoles.Add(role);
        }

        // 3. 逐个检查 [Authorize]（AND 关系）
        foreach (var requiredRoles in authorizeRoles)
        {
            if (requiredRoles.Length == 0)
            {
                // 无参 [Authorize] —— 只要求有任意角色（已认证）
                if (userRoles.Count == 0)
                {
                    await NotifyUnauthorizedAsync(provider, context, requiredRoles).ConfigureAwait(false);
                    return false;
                }

                continue;
            }

            // 有参 [Authorize] —— 要求拥有指定角色之一（OR）
            var satisfied = false;
            foreach (var role in requiredRoles)
            {
                if (userRoles.Contains(role))
                {
                    satisfied = true;
                    break;
                }
            }

            if (!satisfied)
            {
                await NotifyUnauthorizedAsync(provider, context, requiredRoles).ConfigureAwait(false);
                return false;
            }
        }

        return true;
    }

    private static async Task NotifyUnauthorizedAsync(
        IAuthorizationProvider? provider,
        BotContext context,
        string[] requiredRoles)
    {
        if (provider is not null)
            await provider.OnUnauthorizedAsync(context, requiredRoles).ConfigureAwait(false);
    }
}
