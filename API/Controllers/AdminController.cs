using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var Users = await userManager.Users.Include(r => r.UserRoles)
                .ThenInclude(r => r.Role).OrderBy(u => u.UserName).Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(R => R.Role.Name).ToList()

                }).ToListAsync();

            return Ok(Users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]

        public async Task<ActionResult> EditRole(string username, [FromQuery] string roles)
        {
            var SelectedRoles = roles.Split(",").ToArray();

            var User = await userManager.FindByNameAsync(username);

            if (User == null) return NotFound("Could not find user");

            var UserRoles = await userManager.GetRolesAsync(User);

            var Result = await userManager.AddToRolesAsync(User, SelectedRoles.Except(UserRoles));

            if (!Result.Succeeded) return BadRequest("Failed to add to roles");

            Result = await userManager.RemoveFromRolesAsync(User, UserRoles.Except(SelectedRoles));

            if (!Result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await userManager.GetRolesAsync(User));
        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or Moderators can see this");
        }
    }
}
