using NapArona.Controllers;
using NapArona.Controllers.Authorization;

namespace NapArona.Example;

/// <summary>
/// 示例授权提供者 —— 根据 QQ 号分配自定义角色。
/// 实际项目中可以查数据库、读配置文件等。
/// </summary>
public class ExampleAuthorizationProvider : IAuthorizationProvider
{
    /// <summary>
    /// 超级用户 QQ 号列表，实际使用时建议从配置中读取
    /// </summary>
    private static readonly HashSet<long> SuperUsers = [123456789, 987654321];

    public Task<IReadOnlySet<string>> GetRolesAsync(BotContext context)
    {
        var roles = new HashSet<string>(StringComparer.Ordinal);

        if (SuperUsers.Contains(context.UserId))
            roles.Add("SuperUser");

        return Task.FromResult<IReadOnlySet<string>>(roles);
    }

    public Task OnUnauthorizedAsync(BotContext context, string[] requiredRoles)
    {
        // 可以在这里回复"权限不足"，也可以静默忽略
        // 这里演示静默忽略
        return Task.CompletedTask;
    }
}
