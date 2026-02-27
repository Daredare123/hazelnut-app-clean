using System;
using System.ComponentModel.DataAnnotations;

namespace HazelnutVeb.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Quantity (Kg)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public double Quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = "Reserved";

        public int ClientId { get; set; }
        public Client? Client { get; set; }
    }
}
