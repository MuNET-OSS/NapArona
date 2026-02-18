using NapPlana.Core.Data.Event.Notice;

namespace NapArona.Controllers.Attributes;

/// <summary>
/// 监听群聊戳一戳事件。
/// </summary>
public sealed class OnGroupPokeAttribute : OnEventAttribute
{
    public OnGroupPokeAttribute() : base(typeof(GroupPokeNoticeEvent))
    {
    }
}

/// <summary>
/// 监听好友戳一戳事件。
/// </summary>
public sealed class OnFriendPokeAttribute : OnEventAttribute
{
    public OnFriendPokeAttribute() : base(typeof(FriendPokeNoticeEvent))
    {
    }
}

/// <summary>
/// 监听群成员增加事件。
/// </summary>
public sealed class OnGroupIncreaseAttribute : OnEventAttribute
{
    public OnGroupIncreaseAttribute() : base(typeof(GroupIncreaseNoticeEvent))
    {
    }
}

/// <summary>
/// 监听群成员减少事件。
/// </summary>
public sealed class OnGroupDecreaseAttribute : OnEventAttribute
{
    public OnGroupDecreaseAttribute() : base(typeof(GroupDecreaseNoticeEvent))
    {
    }
}
