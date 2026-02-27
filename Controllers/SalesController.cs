using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;
using HazelnutVeb.Services;

namespace HazelnutVeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public SalesController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var sales = await _context.Sales.ToListAsync() ?? new List<Sale>();
                return View(sales);
            }
            catch (Exception)
            {
                return View(new List<Sale>());
            }
        }

        public IActionResult Create()
        {
            return View(new Sale { Date = DateTime.Now });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sale sale)
        {
            if (!ModelState.IsValid)
            {
                return View(sale);
            }

            try 
            {
                // Fetch Inventory (create if not exists, but should exist from Program.cs)
                var inventory = await _context.Inventory.FirstOrDefaultAsync();
                if (inventory == null)
                {
                    inventory = new Inventory { TotalKg = 0 };
                    _context.Inventory.Add(inventory);
                    await _context.SaveChangesAsync();
                }

                // Inventory Validation
                if (sale.QuantityKg > inventory.TotalKg)
                {
                    ModelState.AddModelError("QuantityKg", $"Not enough inventory. Current stock: {inventory.TotalKg:N2} kg");
                    return View(sale);
                }

                // Calculate Total safely
                sale.Total = sale.QuantityKg * sale.PricePerKg;

                // Ensure Date is safely handled for PostgreSQL mapped UTC
                if (sale.Date != DateTime.MinValue)
                {
                    sale.Date = DateTime.SpecifyKind(sale.Date, DateTimeKind.Utc);
                }
                else 
                {
                    sale.Date = DateTime.UtcNow;
                }

                // Add Sale
                _context.Sales.Add(sale);
                
                // Update Inventory
                inventory.TotalKg -= sale.QuantityKg;
                _context.Update(inventory);
                
                await _context.SaveChangesAsync();

                if (inventory.TotalKg <= 5)
                {
                    await _notificationService.SendLowInventoryNotification(inventory.TotalKg);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(sale);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == id);
                if (sale != null)
                {
                    var inventory = await _context.Inventory.FirstOrDefaultAsync();
                    if (inventory == null)
                    {
                        inventory = new Inventory { TotalKg = 0 };
                        _context.Inventory.Add(inventory);
                        await _context.SaveChangesAsync();
                    }
                    
                    // Revert the inventory
                    inventory.TotalKg += sale.QuantityKg;
                    _context.Update(inventory);
                    
                    _context.Sales.Remove(sale);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
