using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Services.Links.MessageQueues;
using Rinkudesu.Services.Links.MessageQueues.Messages;
using Rinkudesu.Services.Links.Repositories;

namespace Rinkudesu.Services.Links.MessageHandlers;

[ExcludeFromCodeCoverage]
public class UserDeletedMessageHandler : IKafkaSubscriberHandler<UserDeletedMessage>
{
    private readonly ILogger<UserDeletedMessageHandler> _logger;
    
    private IServiceScope? scope;

    public UserDeletedMessageHandler(ILogger<UserDeletedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Handle(UserDeletedMessage rawMessage, CancellationToken cancellationToken = default)
    {
        if (scope is null) throw new InvalidOperationException("Scope was not set before handling");

        var repository = scope.ServiceProvider.GetRequiredService<ILinkRepository>();
        try
        {
            await repository.ForceRemoveAllUserLinks(rawMessage.UserId, cancellationToken);
            return true;
        }
        catch (DbUpdateException e)
        {
            _logger.LogWarning(e, "Failed to handle user deletion: '{UserId}'", rawMessage.UserId.ToString());
            return false;
        }
    }

    public UserDeletedMessage Parse(string rawMessage) => System.Text.Json.JsonSerializer.Deserialize<UserDeletedMessage>(rawMessage) ?? throw new FormatException("Unable to parse user deleted message");

    public IKafkaSubscriberHandler<UserDeletedMessage> SetScope(IServiceScope serviceScope)
    {
        scope = serviceScope;
        return this;
    }

    public string Topic => Constants.TOPIC_USER_DELETED;
}
