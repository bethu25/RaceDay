using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Middleware;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Defines the profile endpoint URLs.
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        // Provides access to the RaceDay database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Allows a logged-in user to view their own profile.
        // GET: /api/Profile
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetProfile()
        {
            // Retrieve the logged-in user's ID from the session.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the corresponding account in the database.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            // Reject the request if the account no longer exists.
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            // Return profile information without exposing the password hash.
            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber
            });
        }

        // Allows a logged-in user to update their own profile.
        // PUT: /api/Profile
        [HttpPut]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            // Retrieve the logged-in user's ID from the session.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the user's own account.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            // Reject the request if the account cannot be found.
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            // Clean up the submitted email address.
            string email = request.Email.Trim();

            // Prevent the user from taking another account's email address.
            bool emailExists = await _context.Users.AnyAsync(u =>
                u.Email == email &&
                u.UserID != userId);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "That email address is already registered."
                });
            }

            // Update the user's permitted profile fields.
            user.FullName = request.FullName.Trim();
            user.Email = email;
            user.PhoneNumber = request.PhoneNumber.Trim();

            // Save the updated profile.
            await _context.SaveChangesAsync();

            // Return the updated profile without exposing the password hash.
            return Ok(new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber
            });
        }
    }
}