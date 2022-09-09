using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Services.Links.MessageQueues.Messages;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.MessageQueues;

[ExcludeFromCodeCoverage]
public static class KafkaProducerExtensions
{
    public static async Task ProduceNewLink(this IKafkaProducer producer, Link link, CancellationToken cancellationToken = default)
        => await producer.Produce(Constants.TOPIC_TICKET_NEW, new LinkCreatedMessage(link.Id, link.CreatingUserId), cancellationToken).ConfigureAwait(false);

    public static async Task ProduceDeletedLink(this IKafkaProducer producer, Link link, CancellationToken cancellationToken = default)
        => await producer.Produce(Constants.TOPIC_TICKET_DELETE, new LinkDeletedMessage(link.Id), cancellationToken).ConfigureAwait(false);
}
