using System;

namespace HazelnutVeb.Models
{
    public class DateRangeReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double TotalRevenue { get; set; }
        public double TotalQuantity { get; set; }
        public int TotalSalesCount { get; set; }
    }
}
