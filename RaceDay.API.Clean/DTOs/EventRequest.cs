using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required to create or update an event.
    public class EventRequest
    {
        // Stores the name of the event.
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        // Stores the date on which the event takes place.
        [Required]
        public DateTime EventDate { get; set; }

        // Stores the event location.
        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        // Provides an optional description of the event.
        [StringLength(500)]
        public string? Description { get; set; }

        // Stores the event distance in kilometres.
        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        // Restricts the event type to Run, Walk or Cycle.
        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Event type must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = string.Empty;
    }
}