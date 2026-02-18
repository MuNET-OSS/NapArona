using NapArona.Controllers;
using NapArona.Controllers.Attributes;

namespace NapArona.Example.Controllers;

/// <summary>
/// 正则命令演示控制器，展示使用正则表达式匹配复杂命令模式
/// </summary>
public class RegexCommandController : BotController
{
    private static readonly Random Random = new();

    /// <summary>
    /// 掷骰子命令，格式：/roll 2d6 表示掷2个6面骰
    /// 使用正则表达式捕获 count 和 sides 两个命名组
    /// </summary>
    [Command(@"^/roll (?<count>\d+)d(?<sides>\d+)$", IsRegex = true)]
    public async Task RollAsync(int count, int sides)
    {
        // 限制最大骰子数量，防止滥用
        if (count > 100)
        {
            await ReplyTextAsync("一次最多只能掷100个骰子哦~");
            return;
        }

        // 限制最大面数
        if (sides > 1000)
        {
            await ReplyTextAsync("骰子面数不能超过1000哦~");
            return;
        }

        var results = new List<int>();
        for (int i = 0; i < count; i++)
        {
            results.Add(Random.Next(1, sides + 1));
        }

        var sum = results.Sum();
        var resultText = string.Join(", ", results);

        await ReplyTextAsync($"掷出了 {count}d{sides}: [{resultText}]，总计: {sum}");
    }

    /// <summary>
    /// 表达式记录命令，使用正则捕获任意表达式文本
    /// </summary>
    [Command(@"^/calc (?<expression>.+)$", IsRegex = true)]
    public async Task CalcAsync(string expression)
    {
        await ReplyTextAsync($"表达式: {expression}");
    }
}
