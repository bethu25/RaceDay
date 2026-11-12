using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    // Stores the information required to create or edit an event category.
    public class CategoryViewModel
    {
        // Identifies the category.
        public int CategoryID { get; set; }

        // Identifies the event to which the category belongs.
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an event.")]
        public int EventID { get; set; }

        // Stores the category name.
        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // Stores the category distance in kilometres.
        [Range(0.01, 1000, ErrorMessage = "Enter a valid distance.")]
        public decimal Distance { get; set; }

        // Stores the category entry fee.
        [Range(0, 100000, ErrorMessage = "Enter a valid entry fee.")]
        public decimal EntryFee { get; set; }

        // Stores the maximum number of participants.
        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaxParticipants { get; set; }
    }
}