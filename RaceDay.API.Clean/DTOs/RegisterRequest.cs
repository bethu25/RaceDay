using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required when a user registers.
    public class RegisterRequest
    {
        // Stores the user's full name.
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

    // Stores the email address used to identify the user.
    [Required(ErrorMessage = "Email address is required.")]  
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Receives the password before it is securely hashed.
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        // Allows the user to register as an Organiser or Participant.
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Organiser|Participant)$",
            ErrorMessage = "Role must be Organiser or Participant.")]
        public string Role { get; set; } = string.Empty;

        // Stores the user's contact number.
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }

}
