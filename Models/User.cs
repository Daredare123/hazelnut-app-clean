using System;
using System.ComponentModel.DataAnnotations;

namespace HazelnutVeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string? FullName { get; set; }

        [StringLength(256)]
        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public string Role { get; set; } = "Client";

        public string? FcmToken { get; set; }
    }
}
