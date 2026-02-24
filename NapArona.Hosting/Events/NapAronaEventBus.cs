using System.Collections.Concurrent;
using NapArona.Hosting.Bot;
using NapArona.Hosting.Sessions;
using NapPlana.Core.Data;
using NapPlana.Core.Data.Event.Message;
using NapPlana.Core.Data.Event.Meta;
using NapPlana.Core.Data.Event.Notice;
using NapPlana.Core.Data.Event.Request;

namespace NapArona.Hosting.Events;

/// <summary>
/// 全局事件总线，聚合所有机器人会话的事件。
/// 每个来自 NapPlana Core 的事件都会被包装为 <see cref="BotEvent{T}"/>，
/// 携带触发事件的机器人 ID 和上下文信息。
/// </summary>
public class NapAronaEventBus
{
    /// <summary>
    /// 存储每个会话的清理委托，用于取消订阅时移除所有事件处理器。
    /// </summary>
    private readonly ConcurrentDictionary<long, Action> _subscriptions = new();

    #region 会话生命周期事件

    /// <summary>
    /// 当一个机器人会话连接成功时触发。
    /// </summary>
    public event Action<long, NapAronaBotContext>? BotConnected;

    /// <summary>
    /// 当一个机器人会话断开连接时触发。
    /// </summary>
    public event Action<long>? BotDisconnected;

    #endregion

    #region 元事件

    /// <summary>
    /// 机器人心跳事件。
    /// </summary>
    public event Action<BotEvent<HeartBeatEvent>>? OnHeartbeat;

    /// <summary>
    /// 机器人生命周期事件。
    /// </summary>
    public event Action<BotEvent<LifeCycleEvent>>? OnLifecycle;

    #endregion

    #region 消息事件

    /// <summary>
    /// 群消息接收事件。
    /// </summary>
    public event Action<BotEvent<GroupMessageEvent>>? OnGroupMessage;

    /// <summary>
    /// 私信接收事件（总）。
    /// </summary>
    public event Action<BotEvent<PrivateMessageEvent>>? OnPrivateMessage;

    /// <summary>
    /// 临时会话私信接收事件。
    /// </summary>
    public event Action<BotEvent<PrivateMessageEvent>>? OnPrivateMessageTemporary;

    /// <summary>
    /// 好友私信接收事件。
    /// </summary>
    public event Action<BotEvent<PrivateMessageEvent>>? OnPrivateMessageFriend;

    #endregion

    #region 自身发送事件

    /// <summary>
    /// 群消息发送事件。
    /// </summary>
    public event Action<BotEvent<MessageSentEvent>>? OnMessageSentGroup;

    /// <summary>
    /// 私聊消息发送事件（总）。
    /// </summary>
    public event Action<BotEvent<PrivateMessageSentEvent>>? OnMessageSentPrivate;

    /// <summary>
    /// 临时会话消息发送事件。
    /// </summary>
    public event Action<BotEvent<PrivateMessageSentEvent>>? OnMessageSentPrivateTemporary;

    /// <summary>
    /// 好友消息发送事件。
    /// </summary>
    public event Action<BotEvent<PrivateMessageSentEvent>>? OnMessageSentPrivateFriend;

    #endregion

    #region 通知事件 - 好友相关

    /// <summary>
    /// 好友添加通知事件。
    /// </summary>
    public event Action<BotEvent<FriendAddNoticeEvent>>? OnFriendAddNotice;

    /// <summary>
    /// 好友消息撤回通知事件。
    /// </summary>
    public event Action<BotEvent<FriendRecallNoticeEvent>>? OnFriendRecallNotice;

    #endregion

    #region 通知事件 - 群管理员

    /// <summary>
    /// 群管理员变动通知事件（总）。
    /// </summary>
    public event Action<BotEvent<GroupAdminNoticeEvent>>? OnGroupAdminNotice;

    /// <summary>
    /// 群管理员设置通知事件。
    /// </summary>
    public event Action<BotEvent<GroupAdminNoticeEvent>>? OnGroupAdminSetNotice;

    /// <summary>
    /// 群管理员取消通知事件。
    /// </summary>
    public event Action<BotEvent<GroupAdminNoticeEvent>>? OnGroupAdminUnsetNotice;

    #endregion

    #region 通知事件 - 群禁言

    /// <summary>
    /// 群禁言通知事件（总）。
    /// </summary>
    public event Action<BotEvent<GroupBanNoticeEvent>>? OnGroupBanNotice;

    /// <summary>
    /// 群禁言设置通知事件。
    /// </summary>
    public event Action<BotEvent<GroupBanNoticeEvent>>? OnGroupBanSetNotice;

    /// <summary>
    /// 群禁言解除通知事件。
    /// </summary>
    public event Action<BotEvent<GroupBanNoticeEvent>>? OnGroupBanLiftNotice;

    #endregion

    #region 通知事件 - 群成员名片

    /// <summary>
    /// 群成员名片更新通知事件。
    /// </summary>
    public event Action<BotEvent<GroupCardEvent>>? OnGroupCardNotice;

    #endregion

    #region 通知事件 - 群成员减少

    /// <summary>
    /// 群成员减少通知事件（总）。
    /// </summary>
    public event Action<BotEvent<GroupDecreaseNoticeEvent>>? OnGroupDecreaseNotice;

    /// <summary>
    /// 群成员主动退群通知事件。
    /// </summary>
    public event Action<BotEvent<GroupDecreaseNoticeEvent>>? OnGroupDecreaseLeaveNotice;

    /// <summary>
    /// 群成员被踢通知事件。
    /// </summary>
    public event Action<BotEvent<GroupDecreaseNoticeEvent>>? OnGroupDecreaseKickNotice;

    /// <summary>
    /// 登录号被踢通知事件。
    /// </summary>
    public event Action<BotEvent<GroupDecreaseNoticeEvent>>? OnGroupDecreaseKickMeNotice;

    #endregion

    #region 通知事件 - 群成员增加

    /// <summary>
    /// 群成员增加通知事件（总）。
    /// </summary>
    public event Action<BotEvent<GroupIncreaseNoticeEvent>>? OnGroupIncreaseNotice;

    /// <summary>
    /// 群成员管理员同意通知事件。
    /// </summary>
    public event Action<BotEvent<GroupIncreaseNoticeEvent>>? OnGroupIncreaseApproveNotice;

    /// <summary>
    /// 群成员管理员邀请通知事件。
    /// </summary>
    public event Action<BotEvent<GroupIncreaseNoticeEvent>>? OnGroupIncreaseInviteNotice;

    #endregion

    #region 通知事件 - 群消息撤回

    /// <summary>
    /// 群消息撤回通知事件。
    /// </summary>
    public event Action<BotEvent<GroupRecallNoticeEvent>>? OnGroupRecallNotice;

    #endregion

    #region 通知事件 - 群文件上传

    /// <summary>
    /// 群文件上传通知事件。
    /// </summary>
    public event Action<BotEvent<GroupUploadNoticeEvent>>? OnGroupUploadNotice;

    #endregion

    #region 通知事件 - 群精华消息

    /// <summary>
    /// 群精华消息通知事件（总）。
    /// </summary>
    public event Action<BotEvent<GroupEssenceNoticeEvent>>? OnGroupEssenceNotice;

    /// <summary>
    /// 群精华消息增加通知事件。
    /// </summary>
    public event Action<BotEvent<GroupEssenceNoticeEvent>>? OnGroupEssenceAddNotice;

    /// <summary>
    /// 群精华消息移除通知事件。
    /// </summary>
    public event Action<BotEvent<GroupEssenceNoticeEvent>>? OnGroupEssenceDeleteNotice;

    #endregion

    #region 通知事件 - 群消息表情点赞

    /// <summary>
    /// 群消息表情点赞通知事件。
    /// </summary>
    public event Action<BotEvent<GroupMsgEmojiLikeNoticeEvent>>? OnGroupMsgEmojiLikeNotice;

    #endregion

    #region 通知事件 - Notify子类型

    /// <summary>
    /// 好友戳一戳通知事件。
    /// </summary>
    public event Action<BotEvent<FriendPokeNoticeEvent>>? OnFriendPokeNotice;

    /// <summary>
    /// 群聊戳一戳通知事件。
    /// </summary>
    public event Action<BotEvent<GroupPokeNoticeEvent>>? OnGroupPoke;

    /// <summary>
    /// 输入状态更新通知事件。
    /// </summary>
    public event Action<BotEvent<InputStatusNoticeEvent>>? OnInputStatusNotice;

    /// <summary>
    /// 群成员头衔变更通知事件。
    /// </summary>
    public event Action<BotEvent<GroupTitleEvent>>? OnGroupTitleNotice;

    /// <summary>
    /// 点赞通知事件。
    /// </summary>
    public event Action<BotEvent<ProfileLikeNoticeEvent>>? OnProfileLikeNotice;

    #endregion
    #region 请求事件

    /// <summary>
    /// 好友添加请求事件。
    /// </summary>
    public event Action<BotEvent<FriendRequestEvent>>? OnFriendRequest;

    /// <summary>
    /// 加群请求事件。
    /// </summary>
    public event Action<BotEvent<GroupRequestEvent>>? OnGroupRequest;

    #endregion

    /// <summary>
    /// 订阅指定机器人会话的所有事件。
    /// 会话的每个事件都会被包装为 <see cref="BotEvent{T}"/> 并转发到全局事件总线。
    /// </summary>
    /// <param name="session">要订阅的机器人会话。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="session"/> 为 null 时抛出。</exception>
    /// <exception cref="InvalidOperationException">当该会话已被订阅时抛出。</exception>
    public void SubscribeTo(Sessions.BotSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.IsIdentified)
            throw new InvalidOperationException("Cannot subscribe to an unidentified session.");

        var selfId = session.SelfId!.Value;

        // 若存在旧订阅（理论上不应发生，IdentifySessionAsync 会先 Unsubscribe），
        // 先执行旧清理委托再替换，避免事件处理器泄漏。
        if (_subscriptions.TryRemove(selfId, out var oldCleanup))
        {
            oldCleanup();
        }

        var handler = session.EventHandler;
        var ctx = session.Context;

        BotEvent<T> Wrap<T>(T ev) => new(selfId, ev, ctx);

        // 元事件
        Action onConnected = () => BotConnected?.Invoke(selfId, ctx);
        Action<HeartBeatEvent> onHeartbeat = ev => OnHeartbeat?.Invoke(Wrap(ev));
        Action<LifeCycleEvent> onLifecycle = ev => OnLifecycle?.Invoke(Wrap(ev));

        handler.OnBotConnected += onConnected;
        handler.OnBotHeartbeat += onHeartbeat;
        handler.OnBotLifeCycle += onLifecycle;

        // 消息事件
        Action<GroupMessageEvent> onGroupMsg = ev => OnGroupMessage?.Invoke(Wrap(ev));
        Action<PrivateMessageEvent> onPrivateMsg = ev => OnPrivateMessage?.Invoke(Wrap(ev));
        Action<PrivateMessageEvent> onPrivateMsgTemp = ev => OnPrivateMessageTemporary?.Invoke(Wrap(ev));
        Action<PrivateMessageEvent> onPrivateMsgFriend = ev => OnPrivateMessageFriend?.Invoke(Wrap(ev));

        handler.OnGroupMessageReceived += onGroupMsg;
        handler.OnPrivateMessageReceived += onPrivateMsg;
        handler.OnPrivateMessageReceivedTemporary += onPrivateMsgTemp;
        handler.OnPrivateMessageReceivedFriend += onPrivateMsgFriend;

        // 自身发送事件
        Action<MessageSentEvent> onMsgSentGroup = ev => OnMessageSentGroup?.Invoke(Wrap(ev));
        Action<PrivateMessageSentEvent> onMsgSentPrivate = ev => OnMessageSentPrivate?.Invoke(Wrap(ev));
        Action<PrivateMessageSentEvent> onMsgSentPrivateTemp = ev => OnMessageSentPrivateTemporary?.Invoke(Wrap(ev));
        Action<PrivateMessageSentEvent> onMsgSentPrivateFriend = ev => OnMessageSentPrivateFriend?.Invoke(Wrap(ev));

        handler.OnMessageSentGroup += onMsgSentGroup;
        handler.OnMessageSentPrivate += onMsgSentPrivate;
        handler.OnMessageSentPrivateTemporary += onMsgSentPrivateTemp;
        handler.OnMessageSentPrivateFriend += onMsgSentPrivateFriend;

        // 好友通知事件
        Action<FriendAddNoticeEvent> onFriendAdd = ev => OnFriendAddNotice?.Invoke(Wrap(ev));
        Action<FriendRecallNoticeEvent> onFriendRecall = ev => OnFriendRecallNotice?.Invoke(Wrap(ev));

        handler.OnFriendAddNoticeReceived += onFriendAdd;
        handler.OnFriendRecallNoticeReceived += onFriendRecall;

        // 群管理员通知事件
        Action<GroupAdminNoticeEvent> onGroupAdmin = ev => OnGroupAdminNotice?.Invoke(Wrap(ev));
        Action<GroupAdminNoticeEvent> onGroupAdminSet = ev => OnGroupAdminSetNotice?.Invoke(Wrap(ev));
        Action<GroupAdminNoticeEvent> onGroupAdminUnset = ev => OnGroupAdminUnsetNotice?.Invoke(Wrap(ev));

        handler.OnGroupAdminNoticeReceived += onGroupAdmin;
        handler.OnGroupAdminSetNoticeReceived += onGroupAdminSet;
        handler.OnGroupAdminUnsetNoticeReceived += onGroupAdminUnset;

        // 群禁言通知事件
        Action<GroupBanNoticeEvent> onGroupBan = ev => OnGroupBanNotice?.Invoke(Wrap(ev));
        Action<GroupBanNoticeEvent> onGroupBanSet = ev => OnGroupBanSetNotice?.Invoke(Wrap(ev));
        Action<GroupBanNoticeEvent> onGroupBanLift = ev => OnGroupBanLiftNotice?.Invoke(Wrap(ev));

        handler.OnGroupBanNoticeReceived += onGroupBan;
        handler.OnGroupBanSetNoticeReceived += onGroupBanSet;
        handler.OnGroupBanLiftNoticeReceived += onGroupBanLift;

        // 群成员名片通知事件
        Action<GroupCardEvent> onGroupCard = ev => OnGroupCardNotice?.Invoke(Wrap(ev));
        handler.OnGroupCardNoticeReceived += onGroupCard;

        // 群成员减少通知事件
        Action<GroupDecreaseNoticeEvent> onGroupDecrease = ev => OnGroupDecreaseNotice?.Invoke(Wrap(ev));
        Action<GroupDecreaseNoticeEvent> onGroupDecreaseLeave = ev => OnGroupDecreaseLeaveNotice?.Invoke(Wrap(ev));
        Action<GroupDecreaseNoticeEvent> onGroupDecreaseKick = ev => OnGroupDecreaseKickNotice?.Invoke(Wrap(ev));
        Action<GroupDecreaseNoticeEvent> onGroupDecreaseKickMe = ev => OnGroupDecreaseKickMeNotice?.Invoke(Wrap(ev));

        handler.OnGroupDecreaseNoticeReceived += onGroupDecrease;
        handler.OnGroupDecreaseLeaveNoticeReceived += onGroupDecreaseLeave;
        handler.OnGroupDecreaseKickNoticeReceived += onGroupDecreaseKick;
        handler.OnGroupDecreaseKickMeNoticeReceived += onGroupDecreaseKickMe;

        // 群成员增加通知事件
        Action<GroupIncreaseNoticeEvent> onGroupIncrease = ev => OnGroupIncreaseNotice?.Invoke(Wrap(ev));
        Action<GroupIncreaseNoticeEvent> onGroupIncreaseApprove = ev => OnGroupIncreaseApproveNotice?.Invoke(Wrap(ev));
        Action<GroupIncreaseNoticeEvent> onGroupIncreaseInvite = ev => OnGroupIncreaseInviteNotice?.Invoke(Wrap(ev));

        handler.OnGroupIncreaseNoticeReceived += onGroupIncrease;
        handler.OnGroupIncreaseApproveNoticeReceived += onGroupIncreaseApprove;
        handler.OnGroupIncreaseInviteNoticeReceived += onGroupIncreaseInvite;

        // 群消息撤回通知事件
        Action<GroupRecallNoticeEvent> onGroupRecall = ev => OnGroupRecallNotice?.Invoke(Wrap(ev));
        handler.OnGroupRecallNoticeReceived += onGroupRecall;

        // 群文件上传通知事件
        Action<GroupUploadNoticeEvent> onGroupUpload = ev => OnGroupUploadNotice?.Invoke(Wrap(ev));
        handler.OnGroupUploadNoticeReceived += onGroupUpload;

        // 群精华消息通知事件
        Action<GroupEssenceNoticeEvent> onGroupEssence = ev => OnGroupEssenceNotice?.Invoke(Wrap(ev));
        Action<GroupEssenceNoticeEvent> onGroupEssenceAdd = ev => OnGroupEssenceAddNotice?.Invoke(Wrap(ev));
        Action<GroupEssenceNoticeEvent> onGroupEssenceDelete = ev => OnGroupEssenceDeleteNotice?.Invoke(Wrap(ev));

        handler.OnGroupEssenceNoticeReceived += onGroupEssence;
        handler.OnGroupEssenceAddNoticeReceived += onGroupEssenceAdd;
        handler.OnGroupEssenceDeleteNoticeReceived += onGroupEssenceDelete;

        // 群消息表情点赞通知事件
        Action<GroupMsgEmojiLikeNoticeEvent> onGroupMsgEmojiLike = ev => OnGroupMsgEmojiLikeNotice?.Invoke(Wrap(ev));
        handler.OnGroupMsgEmojiLikeNoticeReceived += onGroupMsgEmojiLike;

        // Notify子类型通知事件
        Action<FriendPokeNoticeEvent> onFriendPoke = ev => OnFriendPokeNotice?.Invoke(Wrap(ev));
        Action<GroupPokeNoticeEvent> onGroupPoke = ev => OnGroupPoke?.Invoke(Wrap(ev));
        Action<InputStatusNoticeEvent> onInputStatus = ev => OnInputStatusNotice?.Invoke(Wrap(ev));
        Action<GroupTitleEvent> onGroupTitle = ev => OnGroupTitleNotice?.Invoke(Wrap(ev));
        Action<ProfileLikeNoticeEvent> onProfileLike = ev => OnProfileLikeNotice?.Invoke(Wrap(ev));

        handler.OnFriendPokeNoticeReceived += onFriendPoke;
        handler.OnGroupPokeNoticeReceived += onGroupPoke;
        handler.OnInputStatusNoticeReceived += onInputStatus;
        handler.OnGroupTitleNoticeReceived += onGroupTitle;
        handler.OnProfileLikeNoticeReceived += onProfileLike;

        // 请求事件
        Action<FriendRequestEvent> onFriendRequest = ev => { var arona = ev.ToArona(); arona.Bot = ctx; OnFriendRequest?.Invoke(Wrap<FriendRequestEvent>(arona)); };
        Action<GroupRequestEvent> onGroupRequest = ev => { var arona = ev.ToArona(); arona.Bot = ctx; OnGroupRequest?.Invoke(Wrap<GroupRequestEvent>(arona)); };

        handler.OnFriendRequestReceived += onFriendRequest;
        handler.OnGroupRequestReceived += onGroupRequest;

        // 直接触发 BotConnected（因为 SubscribeTo 在 lifecycle 事件解析之后调用，
        // Core 的 OnBotConnected 已经触发过了，此时订阅已来不及捕获）
        BotConnected?.Invoke(selfId, ctx);

        // 注册清理委托
        _subscriptions[selfId] = () =>
        {
            handler.OnBotConnected -= onConnected;
            handler.OnBotHeartbeat -= onHeartbeat;
            handler.OnBotLifeCycle -= onLifecycle;

            handler.OnGroupMessageReceived -= onGroupMsg;
            handler.OnPrivateMessageReceived -= onPrivateMsg;
            handler.OnPrivateMessageReceivedTemporary -= onPrivateMsgTemp;
            handler.OnPrivateMessageReceivedFriend -= onPrivateMsgFriend;

            handler.OnMessageSentGroup -= onMsgSentGroup;
            handler.OnMessageSentPrivate -= onMsgSentPrivate;
            handler.OnMessageSentPrivateTemporary -= onMsgSentPrivateTemp;
            handler.OnMessageSentPrivateFriend -= onMsgSentPrivateFriend;

            handler.OnFriendAddNoticeReceived -= onFriendAdd;
            handler.OnFriendRecallNoticeReceived -= onFriendRecall;

            handler.OnGroupAdminNoticeReceived -= onGroupAdmin;
            handler.OnGroupAdminSetNoticeReceived -= onGroupAdminSet;
            handler.OnGroupAdminUnsetNoticeReceived -= onGroupAdminUnset;

            handler.OnGroupBanNoticeReceived -= onGroupBan;
            handler.OnGroupBanSetNoticeReceived -= onGroupBanSet;
            handler.OnGroupBanLiftNoticeReceived -= onGroupBanLift;

            handler.OnGroupCardNoticeReceived -= onGroupCard;

            handler.OnGroupDecreaseNoticeReceived -= onGroupDecrease;
            handler.OnGroupDecreaseLeaveNoticeReceived -= onGroupDecreaseLeave;
            handler.OnGroupDecreaseKickNoticeReceived -= onGroupDecreaseKick;
            handler.OnGroupDecreaseKickMeNoticeReceived -= onGroupDecreaseKickMe;

            handler.OnGroupIncreaseNoticeReceived -= onGroupIncrease;
            handler.OnGroupIncreaseApproveNoticeReceived -= onGroupIncreaseApprove;
            handler.OnGroupIncreaseInviteNoticeReceived -= onGroupIncreaseInvite;

            handler.OnGroupRecallNoticeReceived -= onGroupRecall;
            handler.OnGroupUploadNoticeReceived -= onGroupUpload;

            handler.OnGroupEssenceNoticeReceived -= onGroupEssence;
            handler.OnGroupEssenceAddNoticeReceived -= onGroupEssenceAdd;
            handler.OnGroupEssenceDeleteNoticeReceived -= onGroupEssenceDelete;

            handler.OnGroupMsgEmojiLikeNoticeReceived -= onGroupMsgEmojiLike;

            handler.OnFriendPokeNoticeReceived -= onFriendPoke;
            handler.OnGroupPokeNoticeReceived -= onGroupPoke;
            handler.OnInputStatusNoticeReceived -= onInputStatus;
            handler.OnGroupTitleNoticeReceived -= onGroupTitle;
            handler.OnProfileLikeNoticeReceived -= onProfileLike;

            handler.OnFriendRequestReceived -= onFriendRequest;
            handler.OnGroupRequestReceived -= onGroupRequest;
        };
    }

    /// <summary>
    /// 取消订阅指定机器人会话的所有事件，并触发 <see cref="BotDisconnected"/> 事件。
    /// </summary>
    /// <param name="session">要取消订阅的机器人会话。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="session"/> 为 null 时抛出。</exception>
    public void UnsubscribeFrom(Sessions.BotSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.IsIdentified) return;

        if (_subscriptions.TryRemove(session.SelfId!.Value, out var cleanup))
        {
            cleanup();
            BotDisconnected?.Invoke(session.SelfId!.Value);
        }
    }
}