using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents a user stored in the database.
    // This class maps to the existing Users table.
    [Table("Users")]
    public class User
    {
        // Primary key. SQL Server generates this value automatically.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        // Stores the user's full name.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Stores the user's email address.
        // The database also enforces uniqueness.
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Stores the password value from the database.
        // Passwords should be hashed before being stored in a real application.
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        // Identifies whether the user is an Organiser or Participant.
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;

        // Stores the user's contact number.
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}