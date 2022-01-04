using System;
using System.Threading;
using System.Threading.Tasks;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Repositories;

public interface ISharedLinkRepository : IRequiresUserInfo<ISharedLinkRepository>
{
    /// <summary>
    /// Creates shared key by which the link can be accessed
    /// </summary>
    /// <param name="linkId">Id of the link to share</param>
    /// <param name="cancellationToken">Optional token to cancel the request</param>
    /// <exception cref="DataNotFoundException">Thrown when link with id <param name="linkId"></param> was not found</exception>
    /// <exception cref="DataAlreadyExistsException">Thrown when link with id <param name="linkId"/> is already shared</exception>
    /// <returns>Generated shareable key</returns>
    Task<string> ShareLinkById(Guid linkId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Removes shared key by which the link can be accessed
    /// </summary>
    /// <param name="linkId">Id of the link to unshare</param>
    /// <param name="cancellationToken">Optional token to cancel the request</param>
    /// <exception cref="DataNotFoundException">Thrown when link with id <param name="linkId"></param> was not found</exception>
    /// <exception cref="DataAlreadyExistsException">Thrown when link with id <param name="linkId"/> is not shared</exception>
    Task UnshareLinkById(Guid linkId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Returns a shareable key for the link with given id. If link is not found or is not shared, a <see cref="DataNotFoundException"/> is thrown.
    /// </summary>
    /// <exception cref="DataNotFoundException">Thrown when the link is not found or not shared</exception>
    /// <returns>Shareable key</returns>
    Task<string> GetKey(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Returns a shareable key for the link with given id. If link is not found or is not shared, <see langword="null"/> is returned.
    /// </summary>
    /// <returns>Shareable key or null if not found</returns>
    Task<string?> TryGetKey(Guid id, CancellationToken cancellationToken = default);
}