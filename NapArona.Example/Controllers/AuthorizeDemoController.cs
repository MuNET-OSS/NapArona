using Microsoft.AspNetCore.Authorization;
using NapArona.Controllers;
using NapArona.Controllers.Attributes;
using NapArona.Controllers.Authorization;

namespace NapArona.Example.Controllers;

/// <summary>
/// 授权演示控制器，展示 [Authorize] 和内置角色的使用
/// </summary>
public class AuthorizeDemoController : BotController
{
    /// <summary>
    /// 群管理员专属：全体禁言
    /// </summary>
    [Command("/shutup")]
    [GroupOnly]
    [Authorize(Roles = $"{BuiltinRoles.GroupAdmin}")]
    public async Task ShutUpAsync()
    {
        await ReplyTextAsync("已开启全体禁言（假装的）");
    }

    /// <summary>
    /// 群主专属：转让群
    /// </summary>
    [Command("/transfer")]
    [GroupOnly]
    [Authorize(Roles = BuiltinRoles.GroupOwner)]
    public async Task TransferAsync()
    {
        await ReplyTextAsync("你是群主，但我才不帮你转让群呢");
    }

    /// <summary>
    /// 自定义角色：需要使用者注册 IAuthorizationProvider 返回 "SuperUser" 角色
    /// </summary>
    [Command("/sudo")]
    [Authorize(Roles = "SuperUser")]
    public async Task SudoAsync(params string[] args)
    {
        await ReplyTextAsync($"[sudo] 已执行: {string.Join(' ', args)}");
    }
}
