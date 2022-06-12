using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWitchRoles()
        {
            var user = await _userManager.Users
                      .Include(r => r.UserRoles)
                      .ThenInclude(r => r.Role)
                      .OrderBy(u => u.UserName)
                      .Select(u => new
                      {
                          u.Id,
                          UserName = u.UserName,
                          Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                      })
                      .ToListAsync();

            return Ok(user);
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound("can not find user");

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, selectRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add roles");
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove roles");
            
            return Ok( await _userManager.GetRolesAsync(user));

        }




        [Authorize(Policy = "PhotoModerateRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotoForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}