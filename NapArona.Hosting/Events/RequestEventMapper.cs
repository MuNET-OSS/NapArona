using NapPlana.Core.Data.Event.Request;
using Riok.Mapperly.Abstractions;

namespace NapArona.Hosting.Events;

/// <summary>
/// Mapperly source-generated mapper，用于将 NapPlana 请求事件映射为 NapArona 增强子类。
/// </summary>
[Mapper]
internal static partial class RequestEventMapper
{
    public static partial AronaFriendRequestEvent ToArona(this FriendRequestEvent source);
    public static partial AronaGroupRequestEvent ToArona(this GroupRequestEvent source);
}
