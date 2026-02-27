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
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Monthly(int? month, int? year)
        {
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            DateTime start = new DateTime(y, m, 1);
            DateTime end = start.AddMonths(1);

            try
            {
                var sales = await _context.Sales
                    .Where(s => s.Date >= start && s.Date < end)
                    .ToListAsync() ?? new List<Sale>();

                var expenses = await _context.Expenses
                    .Where(e => e.Date >= start && e.Date < end)
                    .ToListAsync() ?? new List<Expense>();

                var model = new MonthlyReportViewModel
                {
                    Month = m,
                    Year = y,
                    TotalSales = sales.Any() ? sales.Sum(s => s.Total) : 0,
                    TotalExpenses = expenses.Any() ? expenses.Sum(e => e.Amount) : 0,
                    TotalKg = sales.Any() ? sales.Sum(s => s.QuantityKg) : 0,
                    TotalTransactions = sales.Count
                };

                model.Profit = model.TotalSales - model.TotalExpenses;

                return View(model);
            }
            catch (Exception)
            {
                return View(new MonthlyReportViewModel
                {
                    Month = m,
                    Year = y,
                    TotalSales = 0,
                    TotalExpenses = 0,
                    TotalKg = 0,
                    TotalTransactions = 0,
                    Profit = 0
                });
            }
        }

        public async Task<IActionResult> Yearly(int year)
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            DateTime start = new DateTime(year, 1, 1);
            DateTime end = start.AddYears(1);

            try
            {
                var sales = await _context.Sales
                    .Where(s => s.Date >= start && s.Date < end)
                    .ToListAsync() ?? new List<Sale>();

                var model = new YearlyReportViewModel
                {
                    Year = year,
                    TotalRevenue = sales.Any() ? sales.Sum(s => s.QuantityKg * s.PricePerKg) : 0,
                    TotalQuantity = sales.Any() ? sales.Sum(s => s.QuantityKg) : 0,
                    TotalSalesCount = sales.Count
                };

                return View(model);
            }
            catch (Exception)
            {
                return View(new YearlyReportViewModel
                {
                    Year = year,
                    TotalRevenue = 0,
                    TotalQuantity = 0,
                    TotalSalesCount = 0
                });
            }
        }

        public async Task<IActionResult> DateRange(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Sales.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(s => s.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(s => s.Date <= endDate.Value);
                }

                var sales = await query.ToListAsync() ?? new List<Sale>();

                var model = new DateRangeReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = sales.Any() ? sales.Sum(s => s.QuantityKg * s.PricePerKg) : 0,
                    TotalQuantity = sales.Any() ? sales.Sum(s => s.QuantityKg) : 0,
                    TotalSalesCount = sales.Count
                };

                return View(model);
            }
            catch (Exception)
            {
                return View(new DateRangeReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = 0,
                    TotalQuantity = 0,
                    TotalSalesCount = 0
                });
            }
        }
    }
}
