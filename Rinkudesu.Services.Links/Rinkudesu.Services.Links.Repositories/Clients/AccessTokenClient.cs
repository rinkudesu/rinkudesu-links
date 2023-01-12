using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rinkudesu.Services.Links.Repositories.Clients;

[ExcludeFromCodeCoverage]
public abstract class AccessTokenClient
{
    private static readonly JsonSerializerOptions jsonOptions = new()
        { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

    protected ILogger<AccessTokenClient> Logger { get; }
    protected HttpClient Client { get; }

    protected AccessTokenClient(HttpClient client, ILogger<AccessTokenClient> logger)
    {
        Client = client;
        Logger = logger;
    }

    public AccessTokenClient SetAccessToken(string? token)
    {
        Client.DefaultRequestHeaders.Authorization = new ("Bearer", token);
        return this;
    }

    protected async Task<TDto?> HandleMessageAndParseDto<TDto>(HttpResponseMessage message, string logId, CancellationToken token) where TDto : class
    {
        if (!message.IsSuccessStatusCode)
        {
            if (message.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogInformation("Object in client {ClientName} with {LogId} not found", GetType().Name, logId);
            }
            else
            {
                Logger.LogWarning("Unexpected response code while querying for object in client {ClientName} with {LogId}: '{StatusCode}'", GetType().Name, logId, message.StatusCode);
            }
            return null;
        }

        try
        {
            var stream = await message.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<TDto>(stream, jsonOptions, token).ConfigureAwait(false);
        }
        catch (JsonException e)
        {
            Logger.LogWarning(e, "Unable to parse object in client {ClientType} with id {LogId}", GetType().Name, logId);
            return null;
        }
    }

    protected static StringContent GetJsonContent<TDto>(TDto dto)
    {
        var message = JsonSerializer.Serialize(dto, jsonOptions);
        return new StringContent(message, Encoding.UTF8, "application/json");
    }
}
