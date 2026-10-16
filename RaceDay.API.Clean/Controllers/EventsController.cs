using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Middleware;
using RaceDay.API.Clean.Models;

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

        // Receives the database context through dependency injection.
        public EventsController(RaceDayDbContext context)
        {
            _context = context;
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

            // Remove the event from the database.
            _context.Events.Remove(eventItem);

            // Save the deletion.
            await _context.SaveChangesAsync();

            // Confirm that the event was deleted.
            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}