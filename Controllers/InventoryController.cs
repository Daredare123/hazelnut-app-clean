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
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public InventoryController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var inventory = await _context.Inventory.FirstOrDefaultAsync();
                if (inventory == null)
                {
                    inventory = new Inventory { TotalKg = 0 };
                    _context.Inventory.Add(inventory);
                    await _context.SaveChangesAsync();
                }
                return View(inventory);
            }
            catch (Exception)
            {
                return View(new Inventory { TotalKg = 0 });
            }
        }

        public async Task<IActionResult> Update()
        {
            try
            {
                var inventory = await _context.Inventory.FirstOrDefaultAsync();
                if (inventory == null)
                {
                    inventory = new Inventory { TotalKg = 0 };
                    _context.Inventory.Add(inventory);
                    await _context.SaveChangesAsync();
                }
                return View(inventory);
            }
            catch (Exception)
            {
                return View(new Inventory { TotalKg = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(Inventory inventory)
        {
            if (!ModelState.IsValid)
            {
                return View(inventory);
            }

            if (inventory.TotalKg < 0)
            {
                ModelState.AddModelError("TotalKg", "Inventory cannot be negative.");
                return View(inventory);
            }

            try
            {
                var dbInventory = await _context.Inventory.FirstOrDefaultAsync();
                if (dbInventory == null)
                {
                    dbInventory = new Inventory { TotalKg = inventory.TotalKg };
                    _context.Inventory.Add(dbInventory);
                }
                else
                {
                    dbInventory.TotalKg = inventory.TotalKg;
                    _context.Update(dbInventory);
                }
                
                await _context.SaveChangesAsync();

                if (dbInventory.TotalKg <= 5)
                {
                    await _notificationService.SendLowInventoryAlert();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(inventory);
            }
        }
    }
}
