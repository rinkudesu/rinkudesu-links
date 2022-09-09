using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.MessageQueues.Messages;

/// <summary>
/// Message sent when a link is deleted.
/// </summary>
[ExcludeFromCodeCoverage]
public class LinkDeletedMessage : LinkMessage
{
    public LinkDeletedMessage()
    {
    }

    public LinkDeletedMessage(Guid linkId) : base(linkId)
    {
    }
}
