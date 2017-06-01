using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
    [ApiVersion("2.0")]
    public class SpeakersV2Controller : SpeakersController
    {
        public SpeakersV2Controller(ICampRepository campRepository, ILogger<SpeakersController> logger, IMapper mapper,
            UserManager<CampUser> userManager) : base(campRepository, logger, mapper, userManager)
        {
        }

        public override IActionResult GetWithCount(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? CampRepository.GetSpeakersByMonikerWithTalks(moniker)
                : CampRepository.GetSpeakersByMoniker(moniker);

            return Ok(new
            {
                currentTime = DateTime.UtcNow,
                count = speakers.Count(),
                results = Mapper.Map<IEnumerable<Speaker2Model>>(speakers)
            });
        }
    }
}