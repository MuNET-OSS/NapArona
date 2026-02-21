using System.Text.RegularExpressions;

namespace NapArona.Controllers.Routing;

public static class CommandParser
{
    public static CommandParseResult? TryParseSlashCommand(string text, string pattern)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(pattern))
        {
            return null;
        }

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        if (!parts[0].Equals(pattern, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // 计算命令名之后的原始文本
        var rawArgs = string.Empty;
        var commandEnd = text.IndexOf(parts[0], StringComparison.Ordinal) + parts[0].Length;
        if (commandEnd < text.Length)
        {
            rawArgs = text[commandEnd..].TrimStart();
        }

        return new CommandParseResult
        {
            CommandName = pattern,
            Args = parts.Length > 1 ? parts[1..] : Array.Empty<string>(),
            RawArgs = rawArgs
        };
    }

    public static RegexParseResult? TryParseRegexCommand(string text, Regex regex)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var match = regex.Match(text);

        if (!match.Success)
        {
            return null;
        }

        var namedGroups = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var groupName in regex.GetGroupNames())
        {
            if (int.TryParse(groupName, out _))
            {
                continue;
            }

            namedGroups[groupName] = match.Groups[groupName].Value;
        }

        return new RegexParseResult
        {
            Match = match,
            NamedGroups = namedGroups
        };
    }
}

public sealed class CommandParseResult
{
    public string CommandName { get; init; } = string.Empty;

    public string[] Args { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 命令名之后的原始文本（未按空格拆分）。
    /// </summary>
    public string RawArgs { get; init; } = string.Empty;
}

public sealed class RegexParseResult
{
    public Match Match { get; init; } = null!;

    public Dictionary<string, string> NamedGroups { get; init; } = new(StringComparer.Ordinal);
}
