using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HazelnutVeb.Models
{
    [Table("Clients")]
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Sale>? Sales { get; set; }

        [Column("Email")]
        public string? Email { get; set; }
    }
}
