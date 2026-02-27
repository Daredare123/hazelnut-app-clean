using System;

namespace HazelnutVeb.Models
{
    public class MonthlyReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public double TotalSales { get; set; }
        public double TotalExpenses { get; set; }
        public double Profit { get; set; }
        public double TotalKg { get; set; }
        public int TotalTransactions { get; set; }
    }
}
