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
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    public class SpeakersController : BaseController
    {
        private readonly ICampRepository _campRepository;
        private readonly ILogger<SpeakersController> _logger;
        private readonly IMapper _mapper;

        public SpeakersController(ICampRepository campRepository, ILogger<SpeakersController> logger, IMapper mapper)
        {
            _campRepository = campRepository;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpGet]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? _campRepository.GetSpeakersByMonikerWithTalks(moniker)
                : _campRepository.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks
                ? _campRepository.GetSpeakerWithTalks(id)
                : _campRepository.GetSpeaker(id);

            if (speaker == null)
            {
                return NotFound();
            }

            if (speaker.Camp.Moniker != moniker)
            {
                BadRequest("Speaker not in specified Camp");
            }

            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody]SpeakerModel model)
        {
            try
            {
                var camp = _campRepository.GetCampByMoniker(moniker);
                if (camp == null)
                {
                    return BadRequest("Could not find Camp");
                }

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                _campRepository.Add(speaker);

                if (await _campRepository.SaveAllAsync())
                {
                    var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                    return Created(url, _mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception thrown while adding speaker: {exception}");
            }

            return BadRequest("Could not add ne speaker");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody]SpeakerModel model)
        {
            try
            {
                var speaker = _campRepository.GetSpeaker(id);
                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker != moniker)
                {
                    return BadRequest("Speaker and Camp do not match");
                }

                _mapper.Map(model, speaker);

                if (await _campRepository.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception thrown while updating speaker: {exception}");
            }

            return BadRequest("Could not update speaker");

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(String moniker, int id)
        {
            try
            {
                var speaker = _campRepository.GetSpeaker(id);
                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker != moniker)
                {
                    return BadRequest("Speaker and Camp do not match");
                }

                _campRepository.Delete(speaker);

                if (await _campRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception thrown while deleting speaker: {exception}");
            }

            return BadRequest("Could not delete speaker");
        }
    }
}
