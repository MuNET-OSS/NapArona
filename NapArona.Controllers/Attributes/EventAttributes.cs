using NapPlana.Core.Data.Event.Notice;
using NapPlana.Core.Data.Event.Request;

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

/// <summary>
/// 监听好友添加请求事件。
/// </summary>
public sealed class OnFriendRequestAttribute : OnEventAttribute
{
    public OnFriendRequestAttribute() : base(typeof(FriendRequestEvent))
    {
    }
}

/// <summary>
/// 监听加群请求事件。
/// </summary>
public sealed class OnGroupRequestAttribute : OnEventAttribute
{
    public OnGroupRequestAttribute() : base(typeof(GroupRequestEvent))
    {
    }
}
