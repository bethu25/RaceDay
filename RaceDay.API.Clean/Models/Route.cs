using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents a route associated with a RaceDay event.
    [Table("Routes")]
    public class Route
    {
        // Primary key generated automatically by SQL Server.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RouteID { get; set; }

        // Identifies the event associated with this route.
        [Required]
        public int EventID { get; set; }

        // Stores the name of the route.
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; } = string.Empty;

        // Stores the route distance in kilometres.
        [Column(TypeName = "decimal(10,2)")]
        public decimal Distance { get; set; }

        // Stores an optional description of the route.
        [StringLength(500)]
        public string? RouteDescription { get; set; }

        // Navigation property linking the route to its event.
        [ForeignKey(nameof(EventID))]
        public Event? Event { get; set; }
    }

}
