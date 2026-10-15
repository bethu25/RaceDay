using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents a category belonging to a RaceDay event.
    [Table("Categories")]
    public class Category
    {
        // Primary key generated automatically by SQL Server.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        // Identifies the event to which this category belongs.
        [Required]
        public int EventID { get; set; }

        // Stores the category name, such as 5KM Run or 10KM Run.
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        // Stores the category distance in kilometres.
        [Column(TypeName = "decimal(10,2)")]
        public decimal Distance { get; set; }

        // Stores the fee participants must pay to enter this category.
        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        // Stores the maximum number of participants allowed.
        public int MaxParticipants { get; set; }

        // Navigation property linking this category to its event.
        public Event? Event { get; set; }
    }

}
