using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents a participant's entry into an event category.
    [Table("Enrolments")]
    public class Enrolment
    {
        // Primary key generated automatically by SQL Server.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrolmentID { get; set; }

        // Identifies the participant who entered the category.
        [Required]
        public int ParticipantID { get; set; }

        // Identifies the category the participant entered.
        [Required]
        public int CategoryID { get; set; }

        // Stores the date the participant entered.
        // The database supplies today's date if no date is provided.
        [Required]
        public DateTime EnrolmentDate { get; set; }

        // Stores the entry status, such as Active.
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // Navigation property linking this entry to its participant.
        [ForeignKey(nameof(ParticipantID))]
        public User? Participant { get; set; }

        // Navigation property linking this entry to its category.
        public Category? Category { get; set; }

        // Navigation property for the result associated with this entry.
        public Result? Result { get; set; }
    }

}
