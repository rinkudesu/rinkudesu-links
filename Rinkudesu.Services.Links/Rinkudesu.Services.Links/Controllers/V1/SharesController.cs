using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Utils;
#pragma warning disable CS1573

namespace Rinkudesu.Services.Links.Controllers.V1;

/// <summary>
/// Controller responsible for management of shareable keys for links
/// </summary>
[ExcludeFromCodeCoverage]
[ApiController]
[ApiVersion("1")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SharesController : ControllerBase
{
    private readonly ILogger<SharesController> _logger;
    private readonly ISharedLinkRepository _repository;

#pragma warning disable CS1591
    public SharesController(ILogger<SharesController> logger, ISharedLinkRepository repository)
#pragma warning restore CS1591
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Retrieves the shareable key for a link with a given id
    /// </summary>
    /// <param name="id">Id of the link to retrieve the key for</param>
    /// <returns>Shareable key</returns>
    /// <response code="404">When link with given id was not found or is not shared</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetKey(Guid id, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("Shareable key retrieval for link {id}", id);
        try
        {
            return Ok(await _repository.SetUserInfo(User.GetUserInfo()).GetKey(id, cancellationToken));
        }
        catch (RepositoryException e)
        {
            _logger.LogWarning(e, "Shareable link for {id} was unable to be retrieved", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Enables sharing of a link and returns the new key.
    /// </summary>
    /// <param name="id">Id of the link to share</param>
    /// <returns>New shareable key</returns>
    /// <response code="404">When link was not found or is already shared</response>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> Share(Guid id, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("Sharing link {id}", id);
        try
        {
            var key = await _repository.SetUserInfo(User.GetUserInfo()).ShareLinkById(id, cancellationToken);
            return CreatedAtAction(nameof(Share), key);
        }
        catch (RepositoryException e)
        {
            _logger.LogWarning(e, "Unable to share link");
            return NotFound();
        }
    }

    /// <summary>
    /// Removes the shareable key from link
    /// </summary>
    /// <param name="id">Id of the link for which to disable sharing</param>
    /// <response code="404">When link with given id was not found or is not shared</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Unshare(Guid id, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope("Unsharing link {id}", id);
        try
        {
            await _repository.SetUserInfo(User.GetUserInfo()).UnshareLinkById(id, cancellationToken);
            return Ok();
        }
        catch (RepositoryException e)
        {
            _logger.LogWarning(e, "Unable to unshare link");
            return NotFound();
        }
    }
}