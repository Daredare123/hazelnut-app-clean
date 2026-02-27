using System;

namespace HazelnutVeb.Models
{
    public class ClientListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double TotalQuantity { get; set; }
        public double TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
    }
}
