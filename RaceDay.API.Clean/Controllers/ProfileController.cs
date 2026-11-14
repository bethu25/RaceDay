using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Middleware;
using RaceDay.API.Clean.Services;

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

        // Handles Azure Blob Storage operations.
        private readonly BlobStorageService _blobStorageService;

        // Defines the Azure container for profile pictures.
        private const string ProfileContainer = "profile-pictures";

        // Receives the required services through dependency injection.
        public ProfileController(
            RaceDayDbContext context,
            BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
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
                user.PhoneNumber,
                user.ProfileImage
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
                user.PhoneNumber,
                user.ProfileImage
            });
        }

        // Allows a logged-in user to upload their own profile picture.
        // POST: /api/Profile/picture
        [HttpPost("picture")]
        [SessionAuthorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile? file)
        {
            // Retrieve the logged-in user's ID from the session.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the user's account.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            // Reject the request if the account cannot be found.
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            // Check whether a file was selected.
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a profile picture."
                });
            }

            // Validate the image before uploading.
            if (!_blobStorageService.IsValidImage(file))
            {
                return BadRequest(new
                {
                    message = "Invalid image. Use JPG, JPEG, PNG or WEBP up to 5 MB."
                });
            }

            // Remember the previous profile picture.
            string? previousPicture = user.ProfileImage;

            // Store the new image in Azure Blob Storage.
            string newPicture;

            try
            {
                newPicture = await _blobStorageService.UploadAsync(
                    ProfileContainer, file);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }

            try
            {
                // Save the new image filename in the user's database record.
                user.ProfileImage = newPicture;

                await _context.SaveChangesAsync();

                // Delete the previous picture after the new one is saved.
                if (!string.IsNullOrWhiteSpace(previousPicture))
                {
                    await _blobStorageService.DeleteAsync(
                        ProfileContainer, previousPicture);
                }

                // Confirm that the profile picture was uploaded.
                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    profileImage = newPicture
                });
            }
            catch
            {
                // Delete the new upload if saving the database record fails.
                await _blobStorageService.DeleteAsync(
                    ProfileContainer, newPicture);

                // Return the user-friendly error response.
                return StatusCode(500, new
                {
                    message = "The profile picture could not be saved."
                });
            }
        }

        // Allows a logged-in user to retrieve their own profile picture.
        // GET: /api/Profile/picture
        [HttpGet("picture")]
        [SessionAuthorize]
        public async Task<IActionResult> GetProfilePicture()
        {
            // Retrieve the logged-in user's ID from the session.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the user's account.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            // Reject the request if the account cannot be found.
            if (user == null)
            {
                return NotFound(new { message = "User profile not found." });
            }

            // Check whether the user has uploaded a profile picture.
            if (string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                return NotFound(new
                {
                    message = "No profile picture has been uploaded."
                });
            }

            // Retrieve the image from the private Azure container.
            var image = await _blobStorageService.DownloadAsync(
                ProfileContainer, user.ProfileImage);

            // Return an error if the image cannot be found.
            if (image == null)
            {
                return NotFound(new
                {
                    message = "Profile picture not found in storage."
                });
            }

            // Return the image using its original content type.
            return File(image.Value.Content, image.Value.ContentType);
        }
    }
}