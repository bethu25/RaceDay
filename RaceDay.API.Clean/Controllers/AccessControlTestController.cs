using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Clean.Middleware;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Defines the base URL for access-control tests.
    [Route("api/[controller]")]
    public class AccessControlTestController : ControllerBase
    {
        // Allows only Organisers to access this endpoint.
        // GET: /api/AccessControlTest/organiser
        [HttpGet("organiser")]
        [SessionAuthorize("Organiser")]
        public IActionResult OrganiserAccess()
        {
            // Return success when the Organiser role is verified.
            return Ok(new
            {
                message = "Organiser access granted."
            });
        }

        // Allows only Participants to access this endpoint.
        // GET: /api/AccessControlTest/participant
        [HttpGet("participant")]
        [SessionAuthorize("Participant")]
        public IActionResult ParticipantAccess()
        {
            // Return success when the Participant role is verified.
            return Ok(new
            {
                message = "Participant access granted."
            });
        }
    }
}