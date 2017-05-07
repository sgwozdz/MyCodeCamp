using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        private readonly ICampRepository _campRepository;
        private readonly ILogger<CampsController> _logger;

        public CampsController(ICampRepository campRepository, ILogger<CampsController> logger)
        {
            _campRepository = campRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            var camps = _campRepository.GetAllCamps();

            return Ok(camps);
        }

        [HttpGet("{id}", Name = "CampGet")]
        public IActionResult Get(int id, bool includeSpeakers = false)
        {
            try
            {
                _logger.LogInformation("Getting a Code Camp");

                var camp = includeSpeakers
                    ? _campRepository.GetCampWithSpeakers(id)
                    : _campRepository.GetCamp(id);

                if (camp == null)
                    return NotFound($"Camp {id} was not found");

                return Ok(camp);
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
