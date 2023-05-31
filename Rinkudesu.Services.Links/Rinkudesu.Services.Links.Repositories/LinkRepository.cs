using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Kafka.Dotnet.Exceptions;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.MessageQueues;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Repositories.QueryModels;

namespace Rinkudesu.Services.Links.Repositories
{
    public class LinkRepository : ILinkRepository
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly LinkDbContext _context;
        private readonly ILogger<LinkRepository> _logger;

        public LinkRepository(LinkDbContext context, ILogger<LinkRepository> logger, IKafkaProducer kafkaProducer)
        {
            _context = context;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<IEnumerable<Link>> GetAllLinksAsync(LinkListQueryModel queryModel, IEnumerable<Guid>? idsLimit = null, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(GetAllLinksAsync)} with query model: {queryModel}");
            return await queryModel.ApplyQueryModel(_context.Links, idsLimit).ToListAsync(token).ConfigureAwait(false);
        }

        public async Task<Link> GetLinkByKeyAsync(string key, CancellationToken token = default)
        {
            try
            {
                return await _context.Links.AsNoTracking().FirstAsync(l => l.ShareableKey == key, token)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                _logger.LogInformation("Unable to get link by shareable key");
                throw new DataNotFoundException();
            }
        }

        public async Task<Link> GetLinkAsync(Guid linkId, Guid? gettingUserId = null, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(GetLinkAsync)} with linkId='{linkId}' and userId='{gettingUserId}'");
            try
            {
                return await _context.Links.FirstAsync(l =>
                    l.Id == linkId && (l.CreatingUserId == gettingUserId ||
                                       l.PrivacyOptions == Link.LinkPrivacyOptions.Public), token).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                _logger.LogInformation($"Link requested was not found in the database");
                throw new DataNotFoundException(linkId);
            }
        }

        public async Task CreateLinkAsync(Link link, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(CreateLinkAsync)} with link: '{link}'");
            link.SetCreateDates();
            try
            {
                var state = new { context = _context, kafka = _kafkaProducer, link };
                _ = await _context.ExecuteInTransaction(state, async (localState, c) => {
                    localState.context.ClearTracked();
                    localState.context.Links.Add(localState.link);
                    await localState.context.SaveChangesAsync(c).ConfigureAwait(false);
                    await localState.kafka.ProduceNewLink(localState.link, CancellationToken.None).ConfigureAwait(false);
                    return true;
                }, cancellationToken: token).ConfigureAwait(false);
            }
            catch (DbUpdateException e)
            {
                if (_context.Links.Any(l => l.LinkUrl == link.LinkUrl && l.CreatingUserId == link.CreatingUserId))
                {
                    _logger.LogWarning(e, "Link with with url '{url}' for user '{userId}' already exists in the database", link.LinkUrl, link.CreatingUserId.ToString());
                    throw new DataAlreadyExistsException(link.Id);
                }
                _logger.LogWarning(e, "Unexpected error occured while adding a new link to the database");
                throw;
            }
            catch (KafkaProduceException e)
            {
                _logger.LogWarning(e, "Failed to publish link creation kafka message");
                throw;
            }
        }

        public async Task UpdateLinkAsync(Link link, Guid updatingUserId, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(UpdateLinkAsync)} with link '{link}' by userId: '{updatingUserId}'");
            var oldLink =
                await _context.Links.FirstOrDefaultAsync(l => l.Id == link.Id && l.CreatingUserId == updatingUserId, token).ConfigureAwait(false);
            if (oldLink is null)
            {
                _logger.LogInformation($"Link '{link.Id}' was unable to be found for user '{updatingUserId}'");
                throw new DataNotFoundException(link.Id);
            }
            oldLink.Update(link);
            _context.Links.Update(oldLink);
            await _context.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task DeleteLinkAsync(Guid linkId, Guid deletingUserId, CancellationToken token = default)
        {
            _logger.LogDebug($"Executing {nameof(DeleteLinkAsync)} with link id '{linkId}' by user '{deletingUserId}'");
            var link = await _context.Links.FirstOrDefaultAsync(l =>
                l.Id == linkId && l.CreatingUserId == deletingUserId, token).ConfigureAwait(false);
            if (link is null)
            {
                _logger.LogInformation($"Link '{linkId}' was unable to be found for user '{deletingUserId}'");
                throw new DataNotFoundException(linkId);
            }
            var state = new { context = _context, kafka = _kafkaProducer, link };
            _ = await _context.ExecuteInTransaction(state, async (localState, c) => {
                localState.context.ClearTracked();
                localState.context.Remove(localState.link);
                await localState.context.SaveChangesAsync(c).ConfigureAwait(false);
                await localState.kafka.ProduceDeletedLink(localState.link, CancellationToken.None).ConfigureAwait(false);
                return true;
            }, cancellationToken: token).ConfigureAwait(false);
        }

        public async Task ForceRemoveAllUserLinks(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing all links for user '{UserId}'", userId.ToString());
            await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Links\" WHERE \"CreatingUserId\" = {userId}", cancellationToken).ConfigureAwait(false);
        }
    }
}
