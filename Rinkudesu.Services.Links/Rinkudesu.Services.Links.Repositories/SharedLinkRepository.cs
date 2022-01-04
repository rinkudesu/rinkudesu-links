using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Repositories;

public class SharedLinkRepository : ISharedLinkRepository
{
    private readonly LinkDbContext _context;

    private UserInfo? userInfoInternal;

    public SharedLinkRepository(LinkDbContext context)
    {
        _context = context;
    }

    public ISharedLinkRepository SetUserInfo(UserInfo userInfo)
    {
        userInfoInternal = userInfo;
        return this;
    }

    public async Task<string> ShareLinkById(Guid linkId, CancellationToken cancellationToken = default) =>
        await _context.Database.CreateExecutionStrategy().ExecuteInTransactionAsync(c => SetKeyInternal(linkId, c),
            c => VerifyKeySetInternal(linkId, c),
            IsolationLevel.Serializable, cancellationToken).ConfigureAwait(false);

    private async Task<string> SetKeyInternal(Guid linkId, CancellationToken cancellationToken)
    {
        var link = await GetLink(linkId, cancellationToken).ConfigureAwait(false);

        if (link is null)
        {
            throw new DataNotFoundException(linkId);
        }
        if (!string.IsNullOrEmpty(link.ShareableKey))
        {
            throw new DataAlreadyExistsException();
        }

        var key = link.GenerateShareableKey();
        TrackKeyChange(link);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return key;
    }

    private async Task<bool> VerifyKeySetInternal(Guid linkId, CancellationToken cancellationToken)
    {
        _context.ClearTracked();
        return await _context.Links.AnyAsync(l => l.Id == linkId && l.ShareableKey != null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UnshareLinkById(Guid linkId, CancellationToken cancellationToken = default)
    {
        var link = await GetLink(linkId, cancellationToken).ConfigureAwait(false);

        if (link is null)
        {
            throw new DataNotFoundException(linkId);
        }
        if (string.IsNullOrEmpty(link.ShareableKey))
        {
            throw new DataAlreadyExistsException();
        }

        link.Unshare();
        TrackKeyChange(link);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetKey(Guid id, CancellationToken cancellationToken = default)
    {
        var link = await GetLink(id, cancellationToken).ConfigureAwait(false);

        if (link is null)
        {
            throw new DataNotFoundException(id);
        }
        if (string.IsNullOrEmpty(link.ShareableKey))
        {
            throw new DataInvalidException(id, "Link is not shared");
        }

        return link.ShareableKey;
    }

    [ExcludeFromCodeCoverage]
    public async Task<string?> TryGetKey(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetKey(id, cancellationToken).ConfigureAwait(false);
        }
        catch (RepositoryException)
        {
            return null;
        }
    }

    private async Task<Link?> GetLink(Guid linkId, CancellationToken cancellationToken)
    {
        VerifyUserInfo();

        return await _context.Links.AsNoTracking().Where(l => l.Id == linkId && l.CreatingUserId == userInfoInternal!.UserId)
            .Select(l => new Link { Id = l.Id, ShareableKey = l.ShareableKey }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    private void TrackKeyChange(Link link)
    {
        _context.Entry(link).Property(l => l.ShareableKey).IsModified = true;
    }

    private void VerifyUserInfo()
    {
        if (userInfoInternal is null)
        {
            throw new InvalidOperationException($"{nameof(SetUserInfo)} must be called first");
        }
    }
}