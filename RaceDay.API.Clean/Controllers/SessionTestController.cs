using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Defines the URL for testing session information.
    [Route("api/[controller]")]
    public class SessionTestController : ControllerBase
    {
        // Checks whether the current request has an authenticated session.
        // GET: /api/SessionTest
        [HttpGet]
        public IActionResult GetSession()
        {
            // Retrieve the user ID saved during login.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Retrieve the user's role saved during login.
            string? userRole = HttpContext.Session.GetString("UserRole");

            // Return an error if no user ID is stored in this session.
            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "No active session was found. Please log in."
                });
            }

            // Return the session information without exposing the cookie value.
            return Ok(new
            {
                message = "Session is active.",
                UserID = userId,
                UserRole = userRole
            });
        }
    }
}