using System;
using System.ComponentModel.DataAnnotations;

namespace DailyManagementSystem.Models
{
    public enum UserRole
    {
        Admin,
        User
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
