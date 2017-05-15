using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Data
{
    public class CampIdentityInitializer
    {
        private readonly RoleManager<IdentityRole> _roleMgr;
        private readonly UserManager<CampUser> _userMgr;

        public CampIdentityInitializer(UserManager<CampUser> userMgr, RoleManager<IdentityRole> roleMgr)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        public async Task Seed()
        {
            var user = await _userMgr.FindByNameAsync("s.gwozdz");

            // Add User
            if (user == null)
            {
                if (!await _roleMgr.RoleExistsAsync("Admin"))
                {
                    var role = new IdentityRole("Admin");
                    role.Claims.Add(new IdentityRoleClaim<string> { ClaimType = "IsAdmin", ClaimValue = "True" });
                    await _roleMgr.CreateAsync(role);
                }

                user = new CampUser
                {
                    UserName = "s.gwozdz",
                    FirstName = "Szymon",
                    LastName = "Gwozdz",
                    Email = "test@test.com"
                };

                var userResult = await _userMgr.CreateAsync(user, "PassW0RD!");
                var roleResult = await _userMgr.AddToRoleAsync(user, "Admin");
                var claimResult = await _userMgr.AddClaimAsync(user, new Claim("SuperUser", "True"));

                if (!userResult.Succeeded || !roleResult.Succeeded || !claimResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to build user and roles");
                }

            }
        }
    }
}
