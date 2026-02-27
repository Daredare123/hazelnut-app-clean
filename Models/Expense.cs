using System;
using System.ComponentModel.DataAnnotations;

namespace HazelnutVeb.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Amount ($)")]
        public double Amount { get; set; }

        // Optionally, to handle currency
        // public decimal Amount { get; set; } // Often preferable for money, but user asked for double.
    }
}
