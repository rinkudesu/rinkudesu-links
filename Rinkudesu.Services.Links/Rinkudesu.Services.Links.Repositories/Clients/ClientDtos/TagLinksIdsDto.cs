using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Clients.ClientDtos;

[ExcludeFromCodeCoverage]
public class TagLinksIdsDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}
