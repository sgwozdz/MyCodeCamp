using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    public class AuthController : Controller
    {
        private readonly CampContext _context;
        private readonly SignInManager<CampUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CampContext context, SignInManager<CampUser> signInManager, ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception thrown while logging in: {exception}");
            }

            return BadRequest("Failed to login");
        }
    }
}

