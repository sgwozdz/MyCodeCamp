using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : BaseController
    {
        private readonly ICampRepository _campRepository;
        private readonly ILogger<CampsController> _logger;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository campRepository, ILogger<CampsController> logger, IMapper mapper)
        {
            _campRepository = campRepository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _campRepository.GetAllCamps();

            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                _logger.LogInformation("Getting a Code Camp");

                var camp = includeSpeakers
                    ? _campRepository.GetCampByMonikerWithSpeakers(moniker)
                    : _campRepository.GetCampByMoniker(moniker);

                if (camp == null)
                    return NotFound($"Camp {moniker} was not found");

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while getting Camp: {exception}");
            }

            return BadRequest("Couldn't get Camp");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Camp model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");

                _campRepository.Add(model);
                if (await _campRepository.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { id = model.Id });
                    return Created(newUri, model);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while saving Camp: {exception}");
            }

            return BadRequest("Couldn't create Camp");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]Camp model)
        {
            try
            {
                _logger.LogInformation("Updating a Code Camp");

                var oldCamp = _campRepository.GetCamp(id);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with an ID of {id}");
                }

                //Map model to the oldCamp
                oldCamp.Name = model.Name ?? oldCamp.Name;
                oldCamp.Description = model.Description ?? oldCamp.Description;
                oldCamp.Location = model.Location ?? oldCamp.Location;
                oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
                oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

                if (await _campRepository.SaveAllAsync())
                {
                    return Ok(oldCamp);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while updating Camp: {exception}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting a Code Camp");

                var oldCamp = _campRepository.GetCamp(id);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find Camp with an ID of {id}");
                }

                _campRepository.Delete(oldCamp);
                if (await _campRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while deleting Camp: {exception}");
            }

            return BadRequest("Couldn't delete Camp");
        }
    }
}