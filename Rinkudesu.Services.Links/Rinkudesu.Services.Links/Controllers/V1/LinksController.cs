using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rinkudesu.Services.Links.DataTransferObjects.V1;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Repositories.QueryModels;
using Rinkudesu.Services.Links.Utils;

namespace Rinkudesu.Services.Links.Controllers.V1
{
    /// <summary>
    /// Links controller responsible for basic management of <see cref="Link"/> objects
    /// </summary>
    [ExcludeFromCodeCoverage]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILinkRepository _repository;
        private readonly ILogger<LinksController> _logger;

#pragma warning disable 1591
        public LinksController(ILinkRepository repository, IMapper mapper, ILogger<LinksController> logger)
#pragma warning restore 1591
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Gets a complete list of available links to which the user has access
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns>List of links</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LinkDto>>> Get([FromQuery] LinkListQueryModel queryModel)
        {
            using var scope = _logger.BeginScope("Requesting all links with query {queryModel}", queryModel);
            queryModel.UserId = User.GetIdAsGuid();
            var links = await _repository.GetAllLinksAsync(queryModel).ConfigureAwait(false);
            return Ok(_mapper.Map<IEnumerable<LinkDto>>(links));
        }

        /// <summary>
        /// Finds a single link and returns its details
        /// </summary>
        /// <param name="linkId">ID of a link to find</param>
        /// <returns>Found link</returns>
        /// <response code="404">When no link was found with matching ID</response>
        [HttpGet("{linkId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkDto>> GetSingle(Guid linkId)
        {
            using var scope = _logger.BeginScope("Requesting a link with id {linkId}", linkId);
            try
            {
                var link = await _repository.GetLinkAsync(linkId, User.GetIdAsGuid()).ConfigureAwait(false);
                return Ok(_mapper.Map<LinkDto>(link));
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Finds a single link by shareable key and returns its details
        /// </summary>
        /// <param name="key">Shareable key of the link</param>
        /// <returns>Found link</returns>
        /// <response code="404">When no link was found with matching key</response>
        [HttpGet("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkDto>> GetSingleByKey(string key)
        {
            using var scope = _logger.BeginScope("Requesting a link with key");
            try
            {
                var link = await _repository.GetLinkByKeyAsync(key);
                return Ok(_mapper.Map<LinkDto>(link));
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Creates a single new link
        /// </summary>
        /// <param name="newLink"></param>
        /// <remarks>Note that id, creating user id, creation date and update date fields will be ignored, even if they are included in the request</remarks>
        /// <returns>Newly created link object</returns>
        /// <response code="400">When object validation failed or the object already exists in the database</response>
        /// <response code="201">When the object was created correctly</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<LinkDto>> Create([FromBody] LinkDto newLink)
        {
            using var scope = _logger.BeginScope("Creating a link {link}", newLink);
            var link = _mapper.Map<Link>(newLink);
            link.CreatingUserId = User.GetIdAsGuid();
            if (!TryValidateModel(link))
            {
                return BadRequest();
            }
            try
            {
                await _repository.CreateLinkAsync(link).ConfigureAwait(false);
                return CreatedAtAction(nameof(Create), new { linkId = link.Id }, link);
            }
            catch (DataAlreadyExistsException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Modifies a single link with matching id
        /// </summary>
        /// <param name="linkId">ID of a link to update</param>
        /// <param name="updatedLink"></param>
        /// <remarks>Note that id, creating user id, creation date and update date fields will be ignored, even if they are included in the request</remarks>
        /// <returns></returns>
        [HttpPost("{linkId:guid}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Update(Guid linkId, [FromBody] LinkDto updatedLink)
        {
            using var scope =
                _logger.BeginScope("Updating a link with id {linkId} with {newLink}", linkId, updatedLink);
            var link = _mapper.Map<Link>(updatedLink);
            link.Id = linkId;
            link.CreatingUserId = User.GetIdAsGuid();
            if (!TryValidateModel(link))
            {
                return BadRequest();
            }
            try
            {
                await _repository.UpdateLinkAsync(link, User.GetIdAsGuid()).ConfigureAwait(false);
                return Ok();
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes a single link with matching id
        /// </summary>
        /// <param name="linkId">ID of a link to remove</param>
        /// <returns></returns>
        [HttpDelete("{linkId:guid}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(Guid linkId)
        {
            using var scope = _logger.BeginScope("Deleting link {linkId}", linkId);
            try
            {
                await _repository.DeleteLinkAsync(linkId, User.GetIdAsGuid()).ConfigureAwait(false);
                return Ok();
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }
    }
}