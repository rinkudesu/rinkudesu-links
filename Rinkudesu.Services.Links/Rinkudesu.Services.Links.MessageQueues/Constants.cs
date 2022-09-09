using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.MessageQueues;

[ExcludeFromCodeCoverage]
internal static class Constants
{
    public const string TOPIC_TICKET_NEW = "links-new";
    public const string TOPIC_TICKET_DELETE = "links-delete";
}
