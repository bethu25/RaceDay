using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Clean.Models
{
    // Represents a participant's result for an event entry.
    [Table("Results")]
    public class Result
    {
        // Primary key generated automatically by SQL Server.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultID { get; set; }


        // Identifies the enrolment associated with this result.
        [Required]
        public int EnrolmentID { get; set; }

        // Stores the participant's finishing time.
        // This value can be empty until a result is recorded.
        [Column(TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        // Stores the participant's finishing position.
        // This value can be empty until positions are recorded.
        public int? Position { get; set; }

        // Navigation property linking the result to its enrolment.
        [ForeignKey(nameof(EnrolmentID))]
        public Enrolment? Enrolment { get; set; }
    }

}

