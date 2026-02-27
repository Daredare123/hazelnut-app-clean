using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using HazelnutVeb.Models;

namespace HazelnutVeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var expenses = await _context.Expenses.ToListAsync() ?? new List<Expense>();
                return View(expenses);
            }
            catch (Exception)
            {
                return View(new List<Expense>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var expense = await _context.Expenses.FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null) return NotFound();
            return View(expense);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Description,Amount")] Expense expense)
        {
            if (!ModelState.IsValid)
            {
                return View(expense);
            }

            try
            {
                if (expense.Date != DateTime.MinValue)
                {
                    expense.Date = DateTime.SpecifyKind(expense.Date, DateTimeKind.Utc);
                }
                else
                {
                    expense.Date = DateTime.UtcNow;
                }

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(expense);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Description,Amount")] Expense expense)
        {
            if (id != expense.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(expense);
            }

            try
            {
                if (expense.Date != DateTime.MinValue)
                {
                    expense.Date = DateTime.SpecifyKind(expense.Date, DateTimeKind.Utc);
                }
                else
                {
                    expense.Date = DateTime.UtcNow;
                }

                _context.Update(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(expense.Id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
                return View(expense);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var expense = await _context.Expenses.FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null) return NotFound();
            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
                if (expense != null)
                {
                    _context.Expenses.Remove(expense);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
