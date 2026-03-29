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
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
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

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == model.Email.ToLower());

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View(model);
            }

            // Compare hashed password natively
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password";
                return View("Login", model);
            }

            // Store user email and Id in session
            HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role ?? "Client");
            
            // To ensure existing [Authorize] attributes keep working, we also set CookieAuth.
            // If you want pure session explicitly as requested for protecting pages, see the explanation.
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName ?? user.Email ?? string.Empty),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role ?? "Client")
            };

            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Client");
            }
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
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string? fullName)
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

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "Email is already registered.";
                return View();
            }

            Console.WriteLine("Register started");
            
            // Create user
            var user = new User
            {
                FullName = fullName ?? "User",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Client"
            };

            _context.Users.Add(user);
            Console.WriteLine("Before SaveChanges");
            
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine("User saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR saving user:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }

            // Link to Clients table
            bool clientExists = await _context.Clients.AnyAsync(c => c.Email == user.Email);
            if (!clientExists)
            {
                var client = new Client
                {
                    Name = user.Email!,
                    Email = user.Email,
                    Phone = "Unknown",
                    City = "Unknown"
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            // Redirect to Login
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }
    }
}