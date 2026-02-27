using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;
using System.Threading.Tasks;

namespace HazelnutVeb.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SaveToken([FromBody] TokenModel model)
        {
            if (string.IsNullOrEmpty(model?.Token))
            {
                return BadRequest("Invalid token.");
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user != null)
            {
                user.FcmToken = model.Token;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound("User not found.");
        }
    }
}
