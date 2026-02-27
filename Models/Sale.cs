using System;

namespace HazelnutVeb.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double QuantityKg { get; set; }
        public double PricePerKg { get; set; }
        public double Total { get; set; }
        public string ClientName { get; set; }
    }
}
