using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required to create or update a route.
    public class RouteRequest
    {
        // Identifies the event associated with the route.
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        // Stores the route name.
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        // Stores the route distance in kilometres.
        [Range(0.01, 1000)]
        public decimal Distance { get; set; }

        // Provides an optional description of the route.
        [StringLength(500)]
        public string? RouteDescription { get; set; }
    }
}