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

    // Defines the enrolment endpoint URLs.
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        // Provides access to the database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Allows users to view enrolments associated with their role.
        // Participants see their own; Organisers see enrolments for their events.
        // GET: /api/Enrolments
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetEnrolments()
        {
            // Retrieve the logged-in user's ID and role.
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("UserRole");

            // Retrieve enrolments with their category and event information.
            var query = _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .AsQueryable();

            // Participants can see only their own enrolments.
            if (role == "Participant")
            {
                query = query.Where(e => e.ParticipantID == userId);
            }
            // Organisers can see enrolments for events they organise.
            else if (role == "Organiser")
            {
                query = query.Where(e =>
                    e.Category!.Event!.OrganiserID == userId);
            }
            else
            {
                return Forbid();
            }

            // Return a simplified response to avoid exposing unrelated user information.
            var enrolments = await query.Select(e => new
            {
                e.EnrolmentID,
                e.ParticipantID,
                e.CategoryID,
                e.EnrolmentDate,
                e.Status
            }).ToListAsync();

            return Ok(enrolments);
        }

        // Allows a Participant to enrol themselves in a category.
        // POST: /api/Enrolments
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> CreateEnrolment(EnrolmentRequest request)
        {
            // Retrieve the logged-in Participant's ID.
            int? participantId = HttpContext.Session.GetInt32("UserID");

            // Confirm that the requested category exists.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == request.CategoryID);

            // Reject an invalid category ID.
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            // Prevent the same Participant from enrolling in the same category twice.
            bool alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
                e.ParticipantID == participantId &&
                e.CategoryID == request.CategoryID);

            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            // Count active enrolments to enforce the category's participant limit.
            int activeEnrolments = await _context.Enrolments.CountAsync(e =>
                e.CategoryID == request.CategoryID &&
                e.Status == "Active");

            // Reject enrolment if the category has reached its participant limit.
            if (activeEnrolments >= category.MaxParticipants)
            {
                return Conflict(new { message = "This category is full." });
            }

            // Create an enrolment using the logged-in Participant's ID.
            var enrolment = new Enrolment
            {
                ParticipantID = participantId!.Value,
                CategoryID = request.CategoryID,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            };

            // Add the enrolment to the database.
            _context.Enrolments.Add(enrolment);

            // Save the enrolment.
            await _context.SaveChangesAsync();

            // Return the new enrolment.
            return Created("", new
            {
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
        }

        // Allows a Participant to cancel their own enrolment.
        // DELETE: /api/Enrolments/1
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            // Retrieve the logged-in Participant's ID.
            int? participantId = HttpContext.Session.GetInt32("UserID");

            // Find an enrolment belonging to this Participant.
            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentID == id &&
                    e.ParticipantID == participantId);

            // Prevent Participants from cancelling someone else's enrolment.
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            // Remove the enrolment.
            _context.Enrolments.Remove(enrolment);

            // Save the deletion.
            await _context.SaveChangesAsync();

            // Confirm the cancellation.
            return Ok(new { message = "Enrolment cancelled successfully." });
        }
    }
}