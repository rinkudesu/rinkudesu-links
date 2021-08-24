using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Repositories.QueryModels;

namespace Rinkudesu.Services.Links.Repositories
{
    public class LinkRepository : ILinkRepository
    {
        private readonly LinkDbContext _context;
        private readonly ILogger _logger;

        public LinkRepository(LinkDbContext context, ILogger<LinkRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Link>> GetAllLinksAsync(LinkListQueryModel queryModel, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(GetAllLinksAsync)} with query model: {queryModel}");
            return await queryModel.ApplyQueryModel(_context.Links).ToListAsync(token);
        }

        public async Task<Link> GetLinkAsync(Guid linkId, string? gettingUserId = null, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(GetLinkAsync)} with linkId='{linkId}' and userId='{gettingUserId}'");
            try
            {
                return await _context.Links.FirstAsync(l =>
                    l.Id == linkId && (l.CreatingUserId == gettingUserId ||
                                       l.PrivacyOptions == Link.LinkPrivacyOptions.Public), token);
            }
            catch (InvalidOperationException)
            {
                _logger.LogInformation($"Link requested was not found in the database");
                throw new DataNotfoundException(linkId);
            }
        }

        public async Task CreateLinkAsync(Link link, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(CreateLinkAsync)} with link: '{link}'");
            link.SetCreateDates();
            _context.Links.Add(link);
            try
            {
                await _context.SaveChangesAsync(token);
            }
            catch (DbUpdateException e)
            {
                if (_context.Links.Any(l => l.Id == link.Id))
                {
                    _logger.LogWarning(e, "Link with id '{linkId}' already exists in the database", link.Id);
                    throw new DataAlreadyExistsException(link.Id);
                }
                _logger.LogWarning(e, "Unexpected error occured while adding a new link to the database");
                throw;
            }
        }

        public async Task UpdateLinkAsync(Link link, string updatingUserId, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(UpdateLinkAsync)} with link '{link}' by userId: '{updatingUserId}'");
            var oldLink =
                await _context.Links.FirstOrDefaultAsync(l => l.Id == link.Id && l.CreatingUserId == updatingUserId, token);
            if (oldLink is null)
            {
                _logger.LogInformation($"Link '{link.Id}' was unable to be found for user '{updatingUserId}'");
                throw new DataNotfoundException(link.Id);
            }
            oldLink.Update(link);
            _context.Links.Update(oldLink);
            await _context.SaveChangesAsync(token);
        }

        public async Task DeleteLinkAsync(Guid linkId, string deletingUserId, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(DeleteLinkAsync)} with link id '{linkId}' by user '{deletingUserId}'");
            var link = await _context.Links.FirstOrDefaultAsync(l =>
                l.Id == linkId && l.CreatingUserId == deletingUserId, token);
            if (link is null)
            {
                _logger.LogInformation($"Link '{linkId}' was unable to be found for user '{deletingUserId}'");
                throw new DataNotfoundException(linkId);
            }
            _context.Links.Remove(link);
            await _context.SaveChangesAsync(token);
        }
    }
}