namespace NapArona.Controllers.Authorization;

/// <summary>
/// 授权提供者接口，用于获取用户的自定义角色。
/// </summary>
public interface IAuthorizationProvider
{
    /// <summary>
    /// 根据当前上下文返回用户拥有的角色集合。
    /// </summary>
    /// <param name="context">当前 Bot 上下文。</param>
    /// <returns>用户拥有的角色集合。</returns>
    Task<IReadOnlySet<string>> GetRolesAsync(BotContext context);

    /// <summary>
    /// 授权失败时调用，默认静默忽略。
    /// </summary>
    /// <param name="context">当前 Bot 上下文。</param>
    /// <param name="requiredRoles">所需的角色。</param>
    Task OnUnauthorizedAsync(BotContext context, string[] requiredRoles) => Task.CompletedTask;
}
