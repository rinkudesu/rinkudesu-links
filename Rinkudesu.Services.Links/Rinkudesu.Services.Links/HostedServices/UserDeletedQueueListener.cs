using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Services.Links.MessageQueues.Messages;

namespace Rinkudesu.Services.Links.HostedServices;

[ExcludeFromCodeCoverage]
public sealed class UserDeletedQueueListener : IHostedService, IAsyncDisposable
{
    private readonly IKafkaSubscriber<UserDeletedMessage> _subscriber;
    private readonly IKafkaSubscriberHandler<UserDeletedMessage> _handler;

    private readonly CancellationTokenSource _cancellationToken;

    public UserDeletedQueueListener(IKafkaSubscriber<UserDeletedMessage> subscriber, IKafkaSubscriberHandler<UserDeletedMessage> handler)
    {
        _subscriber = subscriber;
        _handler = handler;
        _cancellationToken = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriber.Subscribe(_handler);
        _subscriber.BeginHandle(_cancellationToken.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationToken.Cancel();
        await _subscriber.StopHandle();
        await _subscriber.Unsubscribe();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationToken.Cancel();
        await _subscriber.DisposeAsync();
    }
}
