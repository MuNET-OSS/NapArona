using System.Collections.Concurrent;
using NapArona.Controllers.Filters;

namespace NapArona.Example.Filters;

/// <summary>
/// 命令冷却 Filter —— 同一用户在冷却时间内重复触发同一命令会被拦截。
/// 演示有状态的 Filter（注意：此 Filter 应注册为 Singleton 以共享状态）。
/// </summary>
public class CooldownFilter : IBotFilter
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastUsage = new();

    /// <summary>
    /// 冷却时间（秒）
    /// </summary>
    private const int CooldownSeconds = 10;

    public Task<bool> OnExecutingAsync(BotFilterContext context)
    {
        var key = $"{context.BotContext.UserId}:{context.ControllerType.Name}.{context.Method.Name}";
        var now = DateTimeOffset.UtcNow;

        if (_lastUsage.TryGetValue(key, out var lastTime)
            && now - lastTime < TimeSpan.FromSeconds(CooldownSeconds))
        {
            return Task.FromResult(false);
        }

        _lastUsage[key] = now;
        return Task.FromResult(true);
    }
}
