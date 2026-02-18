using NapArona.Controllers.Filters;

namespace NapArona.Example.Filters;

/// <summary>
/// 群白名单 Filter —— 只允许指定群使用标记了此 Filter 的控制器/命令。
/// 实际项目中可以从数据库或配置文件读取白名单。
/// </summary>
public class GroupWhitelistFilter : IBotFilter
{
    /// <summary>
    /// 允许使用的群号列表，实际使用时建议从配置或数据库读取
    /// </summary>
    private static readonly HashSet<long> AllowedGroups = [111111111, 222222222];

    public Task<bool> OnExecutingAsync(BotFilterContext context)
    {
        // 私聊不受限制
        if (context.BotContext.GroupId is not { } groupId)
            return Task.FromResult(true);

        return Task.FromResult(AllowedGroups.Contains(groupId));
    }
}
