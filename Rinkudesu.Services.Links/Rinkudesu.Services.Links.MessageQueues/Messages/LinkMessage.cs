using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Services.Links.MessageQueues.Messages;

/// <summary>
/// Generic message indicating a change with a link.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class LinkMessage : GenericKafkaMessage
{
    [JsonPropertyName("link_id")]
    public Guid LinkId { get; set; }

    protected LinkMessage()
    {
    }

    protected LinkMessage(Guid linkId)
    {
        LinkId = linkId;
    }
}
