using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HazelnutVeb.Data;
using HazelnutVeb.Models;

namespace HazelnutVeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                if (user.Role != "Admin")
                {
                    user.Role = "Admin";
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToClient(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                // Prevent Admin from demoting themselves.
                if (user.Email != User.Identity?.Name)
                {
                    if (user.Role == "Admin")
                    {
                        user.Role = "Client";
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
