using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rinkudesu.Services.Links.MessageQueues.Messages;

/// <summary>
/// Message sent when a new link is created for a user.
/// </summary>
[ExcludeFromCodeCoverage]
public class LinkCreatedMessage : LinkMessage
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    public LinkCreatedMessage()
    {
    }

    public LinkCreatedMessage(Guid linkId, Guid userId) : base(linkId)
    {
        UserId = userId;
    }
}
