using NapArona.Controllers;
using NapArona.Controllers.Attributes;

namespace NapArona.Example.Controllers;

/// <summary>
/// 基础命令演示控制器，展示基本的命令处理功能
/// </summary>
public class BasicCommandController : BotController
{
    /// <summary>
    /// 简单的 ping 命令，测试机器人是否响应
    /// </summary>
    [Command("/ping")]
    public async Task PingAsync()
    {
        await ReplyTextAsync("pong!");
    }

    /// <summary>
    /// 回声命令，将用户的文本原样返回
    /// </summary>
    [Command("/echo")]
    public async Task EchoAsync(string text)
    {
        await ReplyTextAsync(text);
    }

    /// <summary>
    /// 问候命令，循环多次发送问候语
    /// </summary>
    [Command("/greet")]
    public async Task GreetAsync(string name, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            await ReplyTextAsync($"你好，{name}！(第 {i + 1} 次问候)");
        }
    }

    /// <summary>
    /// 信息命令，显示当前上下文信息
    /// </summary>
    [Command("/info")]
    public async Task InfoAsync()
    {
        var info = $"""
            当前上下文信息：
            - 机器人QQ: {Context.SelfId}
            - 群号: {Context.GroupId?.ToString() ?? "(私聊)"}
            - 用户QQ: {Context.UserId}
            - 消息内容: {Context.TextContent}
            """;
        await ReplyTextAsync(info);
    }
}
