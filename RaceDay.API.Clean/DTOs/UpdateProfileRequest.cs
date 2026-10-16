using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information a user can change in their profile.
    public class UpdateProfileRequest
    {
        // Stores the user's updated full name.
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Stores the user's updated email address.
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Stores the user's updated phone number.
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}