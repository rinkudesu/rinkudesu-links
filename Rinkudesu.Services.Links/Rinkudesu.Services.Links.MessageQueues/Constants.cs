using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.MessageQueues;

[ExcludeFromCodeCoverage]
public static class Constants
{
#pragma warning disable CA1707
    public const string TOPIC_TICKET_NEW = "links-new";
    public const string TOPIC_TICKET_DELETE = "links-delete";
    public const string TOPIC_USER_DELETED = "users-delete";
#pragma warning restore CA1707
}
