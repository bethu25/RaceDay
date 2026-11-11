using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    // Stores the information entered when an organiser creates an event.
    public class EventCreateViewModel
    {
        // Stores the event name.
        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        // Stores the event date.
        [Required(ErrorMessage = "Please select an event date.")]
        public DateTime EventDate { get; set; }

        // Stores where the event will take place.
        [Required(ErrorMessage = "Please enter the event location.")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        // Stores an optional event description.
        [StringLength(500)]
        public string? Description { get; set; }

        // Stores the event distance in kilometres.
        [Range(0.01, 1000, ErrorMessage = "Distance must be between 0.01 and 1000 km.")]
        public decimal Distance { get; set; }

        // Stores the event type.
        [Required(ErrorMessage = "Please select an event type.")]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Event type must be Run, Walk or Cycle.")]
        public string EventType { get; set; } = string.Empty;
    }
}