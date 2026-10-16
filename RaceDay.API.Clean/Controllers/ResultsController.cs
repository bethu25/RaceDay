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

    // Defines the results endpoint URLs.
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        // Provides access to the database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Allows users to view results associated with their role.
        // Participants see their own; Organisers see results for their events.
        // GET: /api/Results
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetResults()
        {
            // Retrieve the logged-in user's ID and role.
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("UserRole");

            // Include enrolment, category and event information.
            var query = _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .AsQueryable();

            // Participants can view only results linked to their own enrolments.
            if (role == "Participant")
            {
                query = query.Where(r =>
                    r.Enrolment!.ParticipantID == userId);
            }
            // Organisers can view results for their own events.
            else if (role == "Organiser")
            {
                query = query.Where(r =>
                    r.Enrolment!.Category!.Event!.OrganiserID == userId);
            }
            else
            {
                return Forbid();
            }

            // Return result details without unrelated user information.
            var results = await query.Select(r => new
            {
                r.ResultID,
                r.EnrolmentID,
                r.FinishTime,
                r.Position
            }).ToListAsync();

            return Ok(results);
        }

        // Allows Organisers to record a result for an enrolment in their event.
        // POST: /api/Results
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateResult(ResultRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Confirm that the enrolment belongs to one of the Organiser's events.
            bool ownsEnrolment = await _context.Enrolments.AnyAsync(e =>
                e.EnrolmentID == request.EnrolmentID &&
                e.Category!.Event!.OrganiserID == organiserId);

            // Reject enrolments that do not belong to this Organiser's events.
            if (!ownsEnrolment)
            {
                return NotFound(new
                {
                    message = "Enrolment not found for this Organiser."
                });
            }

            // Prevent a second result for the same enrolment.
            bool resultExists = await _context.Results.AnyAsync(r =>
                r.EnrolmentID == request.EnrolmentID);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            // Create the result.
            var result = new Result
            {
                EnrolmentID = request.EnrolmentID,
                FinishTime = request.FinishTime,
                Position = request.Position
            };

            // Add the result to the database.
            _context.Results.Add(result);

            // Save the new result.
            await _context.SaveChangesAsync();

            // Return the created result.
            return Created("", new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position
            });
        }

        // Allows Organisers to update results for their own events.
        // PUT: /api/Results/1
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, ResultRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find the result and confirm event ownership.
            var result = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(r =>
                    r.ResultID == id &&
                    r.Enrolment!.Category!.Event!.OrganiserID == organiserId);

            // Return 404 if the result does not belong to this Organiser.
            if (result == null)
            {
                return NotFound(new { message = "Result not found." });
            }

            // Check that the submitted enrolment belongs to this Organiser.
            bool ownsEnrolment = await _context.Enrolments.AnyAsync(e =>
                e.EnrolmentID == request.EnrolmentID &&
                e.Category!.Event!.OrganiserID == organiserId);

            if (!ownsEnrolment)
            {
                return NotFound(new { message = "Enrolment not found for this Organiser." });
            }

            // Prevent changing the result to an enrolment that already has another result.
            bool duplicateResult = await _context.Results.AnyAsync(r =>
                r.EnrolmentID == request.EnrolmentID &&
                r.ResultID != id);

            if (duplicateResult)
            {
                return Conflict(new
                {
                    message = "Another result already exists for this enrolment."
                });
            }

            // Update the result information.
            result.EnrolmentID = request.EnrolmentID;
            result.FinishTime = request.FinishTime;
            result.Position = request.Position;

            // Save the changes.
            await _context.SaveChangesAsync();

            // Return the updated result.
            return Ok(new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position
            });
        }
    }
}