using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.QueryModels;
using Rinkudesu.Services.Links.Repositories.Exceptions;

namespace Rinkudesu.Services.Links.Repositories
{
    /// <summary>
    /// Interface providing the repository capabilities for the <see cref="Link"/> object
    /// </summary>
    public interface ILinkRepository
    {
        /// <summary>
        /// Returns an enumerable of all links filtered by the provided query model
        /// </summary>
        /// <param name="queryModel">A query filter for the list</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable enumerable containing filtered links</returns>
        Task<IEnumerable<Link>> GetAllLinksAsync(LinkListQueryModel queryModel, CancellationToken token = default);

        /// <summary>
        /// Returns a single link
        /// </summary>
        /// <param name="linkId">ID of a link requested</param>
        /// <param name="gettingUserId">ID of the user requesting the link, or null if anonymous</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Link requested</returns>
        /// <exception cref="DataNotFoundException">Thrown when no link exists with given <param name="linkId"></param></exception>
        Task<Link> GetLinkAsync(Guid linkId, Guid? gettingUserId = null, CancellationToken token = default);
        /// <summary>
        /// Creates the new link as provided
        /// </summary>
        /// <param name="link">New link to create</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        /// <exception cref="DataAlreadyExistsException">Thrown when a duplicate primary key has been detected</exception>
        Task CreateLinkAsync(Link link, CancellationToken token = default);
        /// <summary>
        /// Updates the link matching the Id of an object in the argument
        /// </summary>
        /// <param name="link">Link to update, matched by the ID value</param>
        /// <param name="updatingUserId">User requesting the change</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        /// <exception cref="DataNotFoundException">Thrown when no link with the same key exists</exception>
        Task UpdateLinkAsync(Link link, Guid updatingUserId, CancellationToken token = default);

        /// <summary>
        /// Deletes the link from the store
        /// </summary>
        /// <param name="linkId">Link id to delete</param>
        /// <param name="deletingUserId">User requesting deletion</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        /// <exception cref="DataNotFoundException">Thrown when no link exists with ID equal to <param name="linkId"></param></exception>
        Task DeleteLinkAsync(Guid linkId, Guid deletingUserId, CancellationToken token = default);
        /// <summary>
        /// Retrieves the link using the shareable key
        /// </summary>
        /// <param name="key">Shareable key to find</param>
        /// <exception cref="DataNotFoundException">If no link was found with the given link</exception>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Found link</returns>
        Task<Link> GetLinkByKeyAsync(string key, CancellationToken token = default);

        /// <summary>
        /// Removes all links for a given user.
        /// </summary>
        /// <remarks>
        /// Designed to be only run by admin jobs. Not intended to be run manually by any user.
        /// Important note is that this action does not send delete messages to queue. That's because it's designed to be run after receiving "User deleted" message, and the assumption is that this message was also received by all other services that store references to links.
        /// </remarks>
        Task ForceRemoveAllUserLinks(Guid userId, CancellationToken cancellationToken = default);
    }
}
