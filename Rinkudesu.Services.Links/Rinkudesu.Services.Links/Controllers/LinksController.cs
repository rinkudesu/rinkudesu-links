using System;
using System.Collections.Generic;
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
    //TODO: documentation
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LinkDto>>> Get([FromQuery] LinkListQueryModel queryModel)
        {
            var links = await _repository.GetAllLinksAsync(queryModel);
            return Ok(_mapper.Map<IEnumerable<LinkDto>>(links));
        }

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