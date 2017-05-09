using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ValidateModel]
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
        public async Task<IActionResult> Post([FromBody]CampModel model)
        {
            try
            {
                _logger.LogInformation("Creating a new Code Camp");

                var camp = _mapper.Map<Camp>(model);

                _campRepository.Add(camp);
                if (await _campRepository.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
                    return Created(newUri, camp);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while saving Camp: {exception}");
            }

            return BadRequest("Couldn't create Camp");
        }

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody]CampModel model)
        {
            try
            {
                _logger.LogInformation("Updating a Code Camp");

                var oldCamp = _campRepository.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with an ID of {moniker}");
                }

                _mapper.Map(model, oldCamp);

                if (await _campRepository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Threw exception while updating Camp: {exception}");
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                _logger.LogInformation("Deleting a Code Camp");

                var oldCamp = _campRepository.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find Camp with an ID of {moniker}");
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