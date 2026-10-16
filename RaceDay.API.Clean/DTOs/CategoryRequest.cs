using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required to create or update a category.
    public class CategoryRequest
    {
        // Identifies the event to which the category belongs.
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        // Stores the category name.
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // Stores the category distance in kilometres.
        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        // Stores the entry fee for the category.
        [Range(0, 100000)]
        public decimal EntryFee { get; set; }

        // Stores the maximum number of participants allowed.
        [Range(1, 1000000)]
        public int MaxParticipants { get; set; }
    }
}