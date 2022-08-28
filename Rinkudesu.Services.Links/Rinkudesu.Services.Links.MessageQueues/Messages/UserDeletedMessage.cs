using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Services.Links.MessageQueues.Messages;

/// <summary>
/// Message sent when a user is deleted.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserDeletedMessage : GenericKafkaMessage
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    public UserDeletedMessage()
    {
    }

    public UserDeletedMessage(Guid userId)
    {
        UserId = userId;
    }
}
