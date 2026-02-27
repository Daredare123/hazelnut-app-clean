using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;

namespace HazelnutVeb.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _context.Clients
                    .Select(c => new ClientListViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        TotalQuantity = c.Sales != null && c.Sales.Any() ? c.Sales.Sum(s => s.QuantityKg) : 0,
                        TotalRevenue = c.Sales != null && c.Sales.Any() ? c.Sales.Sum(s => s.QuantityKg * s.PricePerKg) : 0,
                        TotalSalesCount = c.Sales != null ? c.Sales.Count : 0
                    })
                    .ToListAsync() ?? new List<ClientListViewModel>();

                return View(clients);
            }
            catch (Exception)
            {
                return View(new List<ClientListViewModel>());
            }
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,City")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,City")] Client client)
        {
            if (id != client.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }

        // GET: Clients/Report
        public async Task<IActionResult> Report(int clientId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var client = await _context.Clients.FindAsync(clientId);
                if (client == null) return NotFound();

                var query = _context.Entry(client).Collection(c => c.Sales!).Query();

                if (startDate.HasValue)
                {
                    query = query.Where(s => s.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(s => s.Date <= endDate.Value);
                }

                var sales = await query.ToListAsync() ?? new List<Sale>();

                var model = new ClientReportViewModel
                {
                    ClientId = clientId,
                    ClientName = client.Name,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalQuantity = sales.Any() ? sales.Sum(s => s.QuantityKg) : 0,
                    TotalRevenue = sales.Any() ? sales.Sum(s => s.QuantityKg * s.PricePerKg) : 0,
                    TotalSalesCount = sales.Count
                };

                return View(model);
            }
            catch (Exception)
            {
                return View(new ClientReportViewModel
                {
                    ClientId = clientId,
                    ClientName = "Unknown Error",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalQuantity = 0,
                    TotalRevenue = 0,
                    TotalSalesCount = 0
                });
            }
        }
    }
}
