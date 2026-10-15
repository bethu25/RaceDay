using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required when a user logs in.
    public class LoginRequest
    {
        // Identifies the account the user wants to access.
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // Receives the password that will be checked against the stored hash.
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

}
