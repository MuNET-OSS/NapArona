using Microsoft.Extensions.Logging;
using NapArona.Controllers.Filters;

namespace NapArona.Example.Filters;

/// <summary>
/// 命令日志 Filter —— 记录每次命令/事件的触发信息。
/// 适合作为全局 Filter 使用。
/// </summary>
public class CommandLoggingFilter : IBotFilter
{
    private readonly ILogger<CommandLoggingFilter> _logger;

    public CommandLoggingFilter(ILogger<CommandLoggingFilter> logger)
    {
        _logger = logger;
    }

    public Task<bool> OnExecutingAsync(BotFilterContext context)
    {
        _logger.LogInformation(
            "[Filter] 用户 {UserId} 在 {Source} 触发了 {Controller}.{Method}",
            context.BotContext.UserId,
            context.BotContext.GroupId is { } gid ? $"群 {gid}" : "私聊",
            context.ControllerType.Name,
            context.Method.Name);

        return Task.FromResult(true);
    }
}
