using NapPlana.Core.Bot;
using NapPlana.Core.Data.API;
using NapPlana.Core.Data.Message;

namespace NapArona.Controllers;

/// <summary>
/// Bot 控制器基类 - Transient 生命周期
/// </summary>
/// <remarks>
/// 所有命令处理器的基类，提供便捷的消息回复方法。
/// Context 属性由 Dispatcher 在调用前设置。
/// </remarks>
public abstract class BotController
{
    /// <summary>
    /// Bot 上下文 - 由 Dispatcher 设置
    /// </summary>
    public BotContext Context { get; internal set; } = null!;

    /// <summary>
    /// 回复文本消息
    /// </summary>
    /// <param name="text">要发送的文本内容</param>
    protected async Task ReplyTextAsync(string text)
    {
        if (Context?.Bot is null)
            throw new InvalidOperationException("BotContext is not initialized.");

        var message = new TextMessage { MessageData = new TextMessageData { Text = text } };
        var messages = new List<MessageBase> { message };

        await ReplyAsync(messages);
    }

    /// <summary>
    /// 回复消息链
    /// </summary>
    /// <param name="messages">要发送的消息链</param>
    protected async Task ReplyAsync(List<MessageBase> messages)
    {
        if (Context?.Bot is null)
            throw new InvalidOperationException("BotContext is not initialized.");

        if (Context.GroupId.HasValue)
        {
            // 群聊消息
            var groupMessage = new GroupMessageSend
            {
                GroupId = Context.GroupId.Value.ToString(),
                Message = messages
            };
            await Context.Bot.SendGroupMessageAsync(groupMessage);
        }
        else
        {
            // 私聊消息
            var privateMessage = new PrivateMessageSend
            {
                UserId = Context.UserId.ToString(),
                Message = messages
            };
            await Context.Bot.SendPrivateMessageAsync(privateMessage);
        }
    }

    /// <summary>
    /// 回复合并转发消息
    /// </summary>
    /// <param name="builder">合并转发消息构建器（外显信息通过 SetSource/SetSummary/SetPrompt/SetNews 设置）</param>
    protected async Task ReplyForwardAsync(ForwardMessageBuilder builder)
    {
        if (Context?.Bot is null)
            throw new InvalidOperationException("BotContext is not initialized.");

        var messages = builder.Build();

        if (Context.GroupId.HasValue)
        {
            await Context.Bot.SendGroupForwardMessageAsync(new GroupForwardMessageSend
            {
                GroupId = Context.GroupId.Value.ToString(),
                Messages = messages,
                Source = builder.Source,
                Summary = builder.Summary,
                Prompt = builder.Prompt,
                News = builder.News
            });
        }
        else
        {
            await Context.Bot.SendPrivateForwardMessageAsync(new PrivateForwardMessageSend
            {
                UserId = Context.UserId.ToString(),
                Messages = messages,
                Source = builder.Source,
                Summary = builder.Summary,
                Prompt = builder.Prompt,
                News = builder.News
            });
        }
    }
}
