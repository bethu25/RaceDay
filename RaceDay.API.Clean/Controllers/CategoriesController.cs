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

    // Defines the category endpoint URLs.
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        // Provides access to the database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Allows logged-in users to view all categories.
        // GET: /api/Categories
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetCategories()
        {
            // Retrieve all categories.
            var categories = await _context.Categories.ToListAsync();

            // Return the categories.
            return Ok(categories);
        }

        // Allows logged-in users to view one category.
        // GET: /api/Categories/1
        [HttpGet("{id:int}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetCategory(int id)
        {
            // Find the requested category.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            // Return 404 when the category does not exist.
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            // Return the category.
            return Ok(category);
        }

        // Allows Organisers to create categories for their own events.
        // POST: /api/Categories
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateCategory(CategoryRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Confirm that the event belongs to this Organiser.
            bool ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventID == request.EventID &&
                e.OrganiserID == userId);

            // Reject the request if the event is missing or belongs to another Organiser.
            if (!ownsEvent)
            {
                return NotFound(new { message = "Event not found for this Organiser." });
            }

            // Create the category from the submitted information.
            var category = new Category
            {
                EventID = request.EventID,
                CategoryName = request.CategoryName,
                Distance = request.Distance,
                EntryFee = request.EntryFee,
                MaxParticipants = request.MaxParticipants
            };

            // Add the category to the database.
            _context.Categories.Add(category);

            // Save the new category.
            await _context.SaveChangesAsync();

            // Return the newly created category.
            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // Allows Organisers to update categories belonging to their events.
        // PUT: /api/Categories/1
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, CategoryRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the category and confirm its event belongs to the Organiser.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == id &&
                    c.EventID == request.EventID &&
                    c.Event!.OrganiserID == userId);

            // Reject missing categories or categories outside the Organiser's ownership.
            if (category == null)
            {
                return NotFound(new { message = "Category not found for this event." });
            }

            // Update the category details.
            category.CategoryName = request.CategoryName;
            category.Distance = request.Distance;
            category.EntryFee = request.EntryFee;
            category.MaxParticipants = request.MaxParticipants;

            // Save the changes.
            await _context.SaveChangesAsync();

            // Return the updated category.
            return Ok(category);
        }

        // Allows Organisers to delete categories belonging to their events.
        // DELETE: /api/Categories/1
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Retrieve the logged-in Organiser's ID.
            int? userId = HttpContext.Session.GetInt32("UserID");

            // Find the category and check event ownership.
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == id &&
                    c.Event!.OrganiserID == userId);

            // Return 404 if the category is not found or is not owned by this Organiser.
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            // Remove the category.
            _context.Categories.Remove(category);

            // Save the deletion.
            await _context.SaveChangesAsync();

            // Confirm deletion.
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}