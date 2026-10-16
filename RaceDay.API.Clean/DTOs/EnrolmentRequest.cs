using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Clean.DTOs
{
    // Defines the information needed to enrol in a category.
    public class EnrolmentRequest
    {
        // Identifies the category the participant wants to enter.
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }
}