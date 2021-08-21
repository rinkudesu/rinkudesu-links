using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.QueryModels;

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
        Task<Link> GetLinkAsync(Guid linkId, string? gettingUserId = null, CancellationToken token = default);
        /// <summary>
        /// Creates the new link as provided
        /// </summary>
        /// <param name="link">New link to create</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        Task CreateLinkAsync(Link link, CancellationToken token = default);
        /// <summary>
        /// Updates the link matching the Id of an object in the argument
        /// </summary>
        /// <param name="link">Link to update, matched by the ID value</param>
        /// <param name="updatingUserId">User requesting the change</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        Task UpdateLinkAsync(Link link, string updatingUserId, CancellationToken token = default);

        /// <summary>
        /// Deletes the link from the store
        /// </summary>
        /// <param name="linkId">Link id to delete</param>
        /// <param name="deletingUserId">User requesting deletion</param>
        /// <param name="token">Token to cancel async operation</param>
        /// <returns>Awaitable task</returns>
        Task DeleteLinkAsync(Guid linkId, string deletingUserId, CancellationToken token = default);
    }
}