using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rinkudesu.Gateways.Utils;
using Rinkudesu.Services.Links.Repositories.Clients.ClientDtos;

namespace Rinkudesu.Services.Links.Repositories.Clients;

[ExcludeFromCodeCoverage]
public class TagsClient : AccessTokenClient
{
    public TagsClient(HttpClient client, ILogger<AccessTokenClient> logger) : base(client, logger)
    {
    }

    public async Task<TagLinksIdsDto[]?> GetLinkIdsForTag(Guid tagId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = await Client.GetAsync($"linkTags/getLinksForTag/{tagId.ToString()}".ToUri(), cancellationToken).ConfigureAwait(false);
            return await HandleMessageAndParseDto<TagLinksIdsDto[]>(message, tagId.ToString(), cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException e)
        {
            Logger.LogWarning(e, "Error while requesting tag with id '{Id}'", tagId.ToString());
            return null;
        }
    }
}
