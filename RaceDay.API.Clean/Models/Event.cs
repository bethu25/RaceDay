using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents an event stored in the existing Events table.
    [Table("Events")]
    public class Event
    {
        // Primary key generated automatically by SQL Server.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }


        // Identifies the user who organised this event.
        [Required]
        public int OrganiserID { get; set; }

        // Stores the event's name.
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        // Stores the date on which the event takes place.
        [Required]
        public DateTime EventDate { get; set; }

        // Stores the event's location.
        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        // Stores an optional description of the event.
        [StringLength(500)]
        public string? Description { get; set; }

        // Stores the main advertised distance of the event in kilometres.
        [Column(TypeName = "decimal(10,2)")]
        public decimal Distance { get; set; }

        // Identifies the event as a Run, Walk, or Cycle.
        [Required]
        [StringLength(10)]
        public string EventType { get; set; } = string.Empty;

        // Navigation property for the user who organised the event.
        public User? Organiser { get; set; }
    }


}
