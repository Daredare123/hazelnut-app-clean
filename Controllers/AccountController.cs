using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;
using Microsoft.AspNetCore.Identity;

namespace HazelnutVeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;
        private readonly SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager,
            SignInManager<Microsoft.AspNetCore.Identity.IdentityUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Console.WriteLine($"Login success: User {model.Email} logged in via Identity.");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine($"Login failed: Identity login failed for {model.Email}");
            ViewBag.Error = "Invalid email or password";
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            var identityUser = new Microsoft.AspNetCore.Identity.IdentityUser 
            { 
                UserName = email, 
                Email = email 
            };

            var result = await _userManager.CreateAsync(identityUser, password);

            if (result.Succeeded)
            {
                // Create linked Client record
                var client = new Client
                {
                    Name = email,
                    Email = email,
                    Phone = "Unknown",
                    City = "Unknown",
                    UserId = identityUser.Id
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                // Sign in the newly registered user
                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // Aggregate Identity errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}