using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Middleware;
using RaceDay.API.Clean.Models;
using RaceDay.API.Clean.Services;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Defines the base URL for event endpoints.
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        // Provides access to the RaceDay database.
        private readonly RaceDayDbContext _context;

        // Handles Azure Blob Storage operations.
        private readonly BlobStorageService _blobStorageService;

        // Receives the required services through dependency injection.
        public EventsController(
            RaceDayDbContext context,
            BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // Allows both roles to view all events.
        // GET: /api/Events
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetEvents()
        {
            // Retrieve all events from the database.
            var events = await _context.Events.ToListAsync();

            // Return the events as JSON.
            return Ok(events);
        }

        // Allows both roles to view one event.
        // GET: /api/Events/1
        [HttpGet("{id:int}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetEvent(int id)
        {
            // Find the event using its ID.
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            // Return 404 if the event does not exist.
            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Return the requested event.
            return Ok(eventItem);
        }

        // Allows Organisers to create events.
        // POST: /api/Events
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {
            // Retrieve the Organiser ID from the logged-in session.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Confirm that the Organiser exists in the database.
            bool organiserExists = await _context.Users.AnyAsync(
                u => u.UserID == organiserId &&
                     u.Role == "Organiser");

            // Reject the request if the session does not belong to an Organiser.
            if (!organiserExists)
            {
                return Forbid();
            }

            // Create an event using the validated request information.
            var eventItem = new Event
            {
                OrganiserID = organiserId.Value,
                EventName = request.EventName,
                EventDate = request.EventDate,
                Location = request.Location,
                Description = request.Description,
                Distance = request.Distance,
                EventType = request.EventType
            };

            // Add the event to the database.
            _context.Events.Add(eventItem);

            // Save the new event.
            await _context.SaveChangesAsync();

            // Return 201 Created with the new event.
            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventID },
                eventItem);
        }

        // Allows Organisers to update events they own.
        // PUT: /api/Events/1
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            EventRequest request)
        {
            // Retrieve the logged-in Organiser ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find an event belonging to this Organiser.
            var eventItem = await _context.Events.FirstOrDefaultAsync(
                e => e.EventID == id &&
                     e.OrganiserID == organiserId);

            // Return 404 if the event does not exist or belongs to someone else.
            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Update the event's editable information.
            eventItem.EventName = request.EventName;
            eventItem.EventDate = request.EventDate;
            eventItem.Location = request.Location;
            eventItem.Description = request.Description;
            eventItem.Distance = request.Distance;
            eventItem.EventType = request.EventType;

            // Save the changes to the database.
            await _context.SaveChangesAsync();

            // Return the updated event.
            return Ok(eventItem);
        }

        // Allows an Organiser to upload or replace their event banner.
        // POST: /api/Events/1/banner
        [HttpPost("{id:int}/banner")]
        [SessionAuthorize("Organiser")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadBanner(
            int id,
            IFormFile? file)
        {
            // Retrieve the logged-in Organiser ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find an event belonging to this Organiser.
            var eventItem = await _context.Events.FirstOrDefaultAsync(
                e => e.EventID == id &&
                     e.OrganiserID == organiserId);

            // Prevent Organisers from changing another person's banner.
            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found or you do not own this event."
                });
            }

            // Check whether an image was selected.
            if (file == null)
            {
                return BadRequest(new
                {
                    message = "Please select an image to upload."
                });
            }

            // Validate the image format and file size.
            if (!_blobStorageService.IsValidImage(file))
            {
                return BadRequest(new
                {
                    message = "Use JPG, JPEG, PNG or WEBP images up to 5 MB."
                });
            }

            // Remember the previous banner filename.
            string? previousBanner = eventItem.BannerImage;

            // Upload the new banner to the private Azure container.
            string newBanner = await _blobStorageService.UploadAsync(
                "event-banners",
                file);

            // Save the new Azure filename in the database.
            eventItem.BannerImage = newBanner;

            try
            {
                // Save the updated event.
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Remove the newly uploaded image if saving fails.
                await _blobStorageService.DeleteAsync(
                    "event-banners",
                    newBanner);

                throw;
            }

            // Delete the previous banner after the replacement succeeds.
            if (!string.IsNullOrWhiteSpace(previousBanner))
            {
                await _blobStorageService.DeleteAsync(
                    "event-banners",
                    previousBanner);
            }

            // Confirm that the banner was uploaded successfully.
            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                bannerImage = newBanner
            });
        }

        // Allows logged-in users to retrieve an event banner.
        // GET: /api/Events/1/banner
        [HttpGet("{id:int}/banner")]
        [SessionAuthorize]
        public async Task<IActionResult> GetBanner(int id)
        {
            // Find the event.
            var eventItem = await _context.Events.FirstOrDefaultAsync(
                e => e.EventID == id);

            // Return 404 if the event does not exist.
            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Check whether the event has a banner.
            if (string.IsNullOrWhiteSpace(eventItem.BannerImage))
            {
                return NotFound(new
                {
                    message = "This event does not have a banner."
                });
            }

            // Download the banner from the private Azure container.
            var image = await _blobStorageService.DownloadAsync(
                "event-banners",
                eventItem.BannerImage);

            // Return 404 if the image cannot be found.
            if (image == null)
            {
                return NotFound(new
                {
                    message = "Event banner image not found."
                });
            }

            // Return the image to the requesting user.
            return File(image.Value.Content, image.Value.ContentType);
        }

        // Allows Organisers to delete events they own.
        // DELETE: /api/Events/1
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            // Retrieve the logged-in Organiser ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find the event belonging to this Organiser.
            var eventItem = await _context.Events.FirstOrDefaultAsync(
                e => e.EventID == id &&
                     e.OrganiserID == organiserId);

            // Return 404 if the event does not exist or belongs to someone else.
            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Remember the banner filename before deleting the event.
            string? bannerImage = eventItem.BannerImage;

            // Remove the event from the database.
            _context.Events.Remove(eventItem);

            // Save the deletion.
            await _context.SaveChangesAsync();

            // Delete the event banner from Azure if one exists.
            if (!string.IsNullOrWhiteSpace(bannerImage))
            {
                await _blobStorageService.DeleteAsync(
                    "event-banners",
                    bannerImage);
            }

            // Confirm that the event was deleted.
            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}