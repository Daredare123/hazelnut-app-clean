using System;

namespace HazelnutVeb.Models
{
    public class ClientReportViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double TotalQuantity { get; set; }
        public double TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
    }
}
