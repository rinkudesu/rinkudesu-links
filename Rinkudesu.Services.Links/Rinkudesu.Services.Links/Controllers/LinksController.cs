﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rinkudesu.Services.Links.DataTransferObjects;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Repositories.QueryModels;

namespace Rinkudesu.Services.Links.Controllers
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILinkRepository _repository;

        public LinksController(ILinkRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
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
            var links = await _repository.GetAllLinksAsync(queryModel);
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
            //TODO: read user id once it's available
            try
            {
                var link = await _repository.GetLinkAsync(linkId);
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
            var link = _mapper.Map<Link>(newLink);
            //TODO: read user id once it's available
            link.CreatingUserId = "CHANGEME";
            if (!TryValidateModel(link))
            {
                return BadRequest();
            }
            try
            {
                await _repository.CreateLinkAsync(link);
                return CreatedAtAction(nameof(GetSingle), new { linkId = link.Id }, link);
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
            var link = _mapper.Map<Link>(updatedLink);
            link.Id = linkId;
            //TODO: read user id once it's available
            if (!TryValidateModel(link))
            {
                return BadRequest();
            }
            try
            {
                await _repository.UpdateLinkAsync(link, "CHANGEME");
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
            //TODO: read user id
            try
            {
                await _repository.DeleteLinkAsync(linkId, "CHANGEME");
                return Ok();
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }
    }
}