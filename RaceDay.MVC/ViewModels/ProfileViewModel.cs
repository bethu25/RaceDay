namespace RaceDay.MVC.ViewModels
{
    // Holds the information displayed on the user's profile page.
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }
    }
}