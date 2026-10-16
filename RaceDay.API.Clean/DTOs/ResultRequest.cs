using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information required to record or update a result.
    public class ResultRequest
    {
        // Identifies the enrolment associated with this result.
        [Range(1, int.MaxValue)]
        public int EnrolmentID { get; set; }

        // Stores the participant's finishing time.
        // Example: 00:45:32 represents 45 minutes and 32 seconds.
        public TimeSpan? FinishTime { get; set; }

        // Stores the participant's finishing position.
        [Range(1, int.MaxValue)]
        public int? Position { get; set; }
    }
}