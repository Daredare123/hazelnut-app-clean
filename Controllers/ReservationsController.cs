using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;
using HazelnutVeb.Services;

namespace HazelnutVeb.Controllers
{
    [Authorize(Roles = "Admin,Client")]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;
        private readonly PushNotificationService _pushNotificationService;

        public ReservationsController(AppDbContext context, NotificationService notificationService, EmailService emailService, PushNotificationService pushNotificationService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var query = _context.Reservations
                    .Include(r => r.Client)
                    .AsQueryable();

                var email = User.Identity?.Name;
                var currentUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

                if (currentUser != null && currentUser.Role != "Admin")
                {
                    query = query.Where(r => r.Client.Email == email);
                }

                var reservations = await query
                    .OrderByDescending(r => r.Date)
                    .ToListAsync() ?? new List<Reservation>();
                return View(reservations);
            }
            catch (Exception)
            {
                return View(new List<Reservation>());
            }
        }

        [Authorize(Roles = "Client")]
        public IActionResult MyOrders()
        {
            return View();
        }

        [Authorize(Roles = "Client")]
        public IActionResult Create()
        {
            return View(new Reservation { Date = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create([Bind("Quantity,Date")] Reservation reservation)
        {
            if (reservation == null)
            {
                ModelState.AddModelError("", "Reservation data is missing.");
                return View(new Reservation { Date = DateTime.UtcNow });
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Unable to verify current user session.");
                return View(reservation);
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
            if (client == null)
            {
                client = new Client
                {
                    Email = email,
                    Name = email,
                    Phone = "Unknown",
                    City = "Unknown"
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                
                // Reload Client
                client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
            }

            if (client != null)
            {
                reservation.ClientId = client.Id;
            }

            if (!ModelState.IsValid)
            {
                return View(reservation);
            }

            try
            {
                if (reservation.Quantity <= 0)
                {
                    ModelState.AddModelError("Quantity", "Quantity must be greater than zero.");
                    return View(reservation);
                }

                reservation.Status = "Pending";

                // Ensure PostgreSQL DateTime strictly enforced dynamically to UTC without nullable errors
                if (reservation.Date != DateTime.MinValue)
                {
                    reservation.Date = DateTime.SpecifyKind(reservation.Date, DateTimeKind.Utc);
                }
                else 
                {
                    reservation.Date = DateTime.UtcNow;
                }

                var inventory = await _context.Inventory.FirstOrDefaultAsync();
                if (inventory == null)
                {
                    inventory = new Inventory { TotalKg = 0 };
                    _context.Inventory.Add(inventory);
                    await _context.SaveChangesAsync();
                }

                if (reservation.Quantity > inventory.TotalKg)
                {
                    ModelState.AddModelError("Quantity", $"Not enough inventory. Current stock: {inventory.TotalKg:N2} kg");
                    return View(reservation);
                }

                _context.Reservations.Add(reservation);
                
                inventory.TotalKg -= reservation.Quantity;
                _context.Update(inventory);
                
                await _context.SaveChangesAsync();

                try
                {
                    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
                    var fullName = user?.FullName ?? client.Name;

                    var admins = await _context.AppUsers
                        .Where(u => u.Role == "Admin" && u.Email != email)
                        .ToListAsync();

                    foreach (var admin in admins)
                    {
                        if (!string.IsNullOrEmpty(admin.Email))
                        {
                            await _emailService.SendEmailAsync(
                                admin.Email,
                                "Нова резервација",
                                $"Имате нова резервација.\n\nКлиент: {fullName}\nКоличина: {reservation.Quantity} кг\nДатум: {reservation.Date:dd.MM.yyyy}\n\nНајавете се во системот за да ја прегледате."
                            );
                        }
                        
                        if (!string.IsNullOrEmpty(admin.FcmToken))
                        {
                            await _pushNotificationService.SendAsync(
                                admin.FcmToken,
                                "Нова резервација",
                                "Имате нова резервација."
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending admin notification emails: {ex.Message}");
                }

                if (inventory.TotalKg <= 5)
                {
                    await _notificationService.SendLowInventoryNotification(inventory.TotalKg);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(reservation);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null || reservation.Status != "Pending") return NotFound();

            reservation.Status = "Approved";
            _context.Update(reservation);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(reservation.Client?.Email))
            {
                try
                {
                    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == reservation.Client.Email);
                    var fullName = user?.FullName ?? reservation.Client.Name;

                    await _emailService.SendEmailAsync(
                        reservation.Client.Email, 
                        "Вашата резервација е прифатена", 
                        $"Почитуван/а {fullName},\n\nВашата резервација од {reservation.Quantity} кг за датум {reservation.Date:dd.MM.yyyy} е успешно прифатена.\n\nВи благодариме."
                    );

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        await _pushNotificationService.SendAsync(
                            user.FcmToken, 
                            "Резервацијата е прифатена", 
                            "Вашата резервација е успешно прифатена."
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending approval email: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null || reservation.Status != "Pending") return NotFound();

            reservation.Status = "Rejected";

            var inventory = await _context.Inventory.FirstOrDefaultAsync();
            if (inventory != null)
            {
                inventory.TotalKg += reservation.Quantity;
                _context.Update(inventory);
            }

            _context.Update(reservation);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(reservation.Client?.Email))
            {
                try
                {
                    var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == reservation.Client.Email);
                    var fullName = user?.FullName ?? reservation.Client.Name;

                    await _emailService.SendEmailAsync(
                        reservation.Client.Email, 
                        "Вашата резервација е одбиена", 
                        $"Почитуван/а {fullName},\n\nЗа жал, вашата резервација од {reservation.Quantity} кг за датум {reservation.Date:dd.MM.yyyy} не можеме да ја реализираме.\n\nВе молиме контактирајте не за повеќе информации."
                    );

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        await _pushNotificationService.SendAsync(
                            user.FcmToken, 
                            "Резервацијата е одбиена", 
                            "Жалиме, вашата резервација е одбиена."
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending rejection email: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete(int id, double pricePerKg)
        {
            try
            {
                var reservation = await _context.Reservations.Include(r => r.Client).FirstOrDefaultAsync(r => r.Id == id);
                if (reservation == null || reservation.Status != "Approved")
                {
                    return NotFound();
                }

                reservation.Status = "Completed";
                _context.Update(reservation);

                var sale = new Sale
                {
                    Date = DateTime.UtcNow,
                    QuantityKg = reservation.Quantity,
                    PricePerKg = pricePerKg,
                    Total = reservation.Quantity * pricePerKg,
                    ClientName = reservation.Client?.Email ?? "Unknown"
                };

                _context.Sales.Add(sale);
                _context.Entry(sale).Property("ClientId").CurrentValue = reservation.ClientId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
                if (reservation == null || (reservation.Status != "Pending" && reservation.Status != "Approved"))
                {
                    return NotFound();
                }

                reservation.Status = "Cancelled";
                _context.Update(reservation);

                var inventory = await _context.Inventory.FirstOrDefaultAsync();
                if (inventory == null)
                {
                    inventory = new Inventory { TotalKg = 0 };
                    _context.Inventory.Add(inventory);
                }

                inventory.TotalKg += reservation.Quantity;
                _context.Update(inventory);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
