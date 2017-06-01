using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [ApiVersion("1.1")]
    [ApiVersion("1.0")]
    public class SpeakersController : BaseController
    {
        protected readonly ICampRepository CampRepository;
        protected readonly ILogger<SpeakersController> Logger;
        protected readonly IMapper Mapper;
        protected readonly UserManager<CampUser> UserManager;

        public SpeakersController(ICampRepository campRepository, ILogger<SpeakersController> logger, IMapper mapper, UserManager<CampUser> userManager)
        {
            CampRepository = campRepository;
            Logger = logger;
            Mapper = mapper;
            UserManager = userManager;
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? CampRepository.GetSpeakersByMonikerWithTalks(moniker)
                : CampRepository.GetSpeakersByMoniker(moniker);

            return Ok(Mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet]
        [MapToApiVersion("1.1")]
        public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? CampRepository.GetSpeakersByMonikerWithTalks(moniker)
                : CampRepository.GetSpeakersByMoniker(moniker);

            return Ok(new
            {
                count = speakers.Count(),
                results = Mapper.Map<IEnumerable<SpeakerModel>>(speakers)
            });
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            var speaker = includeTalks
                ? CampRepository.GetSpeakerWithTalks(id)
                : CampRepository.GetSpeaker(id);

            if (speaker == null)
            {
                return NotFound();
            }

            if (speaker.Camp.Moniker != moniker)
            {
                BadRequest("Speaker not in specified Camp");
            }

            return Ok(Mapper.Map<SpeakerModel>(speaker));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody]SpeakerModel model)
        {
            try
            {
                var camp = CampRepository.GetCampByMoniker(moniker);
                if (camp == null)
                {
                    return BadRequest("Could not find Camp");
                }

                var speaker = Mapper.Map<Speaker>(model);
                speaker.Camp = camp;

                var campUser = await UserManager.FindByNameAsync(User.Identity.Name);
                if (campUser != null)
                {
                    speaker.User = campUser;
                    CampRepository.Add(speaker);

                    if (await CampRepository.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new { moniker = camp.Moniker, id = speaker.Id });
                        return Created(url, Mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError($"Exception thrown while adding speaker: {exception}");
            }

            return BadRequest("Could not add ne speaker");
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody]SpeakerModel model)
        {
            try
            {
                var speaker = CampRepository.GetSpeaker(id);
                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker != moniker)
                {
                    return BadRequest("Speaker and Camp do not match");
                }

                if (speaker.User.UserName != User.Identity.Name)
                {
                    return Forbid();
                }

                Mapper.Map(model, speaker);

                if (await CampRepository.SaveAllAsync())
                {
                    return Ok(Mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception exception)
            {
                Logger.LogError($"Exception thrown while updating speaker: {exception}");
            }

            return BadRequest("Could not update speaker");

        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = CampRepository.GetSpeaker(id);
                if (speaker == null)
                {
                    return NotFound();
                }

                if (speaker.Camp.Moniker != moniker)
                {
                    return BadRequest("Speaker and Camp do not match");
                }

                if (speaker.User.UserName != User.Identity.Name)
                {
                    return Forbid();
                }

                CampRepository.Delete(speaker);

                if (await CampRepository.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception exception)
            {
                Logger.LogError($"Exception thrown while deleting speaker: {exception}");
            }

            return BadRequest("Could not delete speaker");
        }
    }
}
