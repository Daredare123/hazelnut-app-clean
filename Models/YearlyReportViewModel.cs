using System;

namespace HazelnutVeb.Models
{
    public class YearlyReportViewModel
    {
        public int Year { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalQuantity { get; set; }
        public int TotalSalesCount { get; set; }
    }
}
