using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.MessageQueues;

[ExcludeFromCodeCoverage]
public static class Constants
{
    public const string TOPIC_TICKET_NEW = "links-new";
    public const string TOPIC_TICKET_DELETE = "links-delete";
    public const string TOPIC_USER_DELETED = "users-delete";
}
