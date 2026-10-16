using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Middleware;
using RaceDayRoute = RaceDay.API.Clean.Models.Route;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Defines the route endpoint URLs.
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        // Provides access to the database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public RoutesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Allows logged-in users to view all routes.
        // GET: /api/Routes
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetRoutes()
        {
            // Retrieve all routes.
            var routes = await _context.Routes.ToListAsync();

            // Return the routes.
            return Ok(routes);
        }

        // Allows logged-in users to view one route.
        // GET: /api/Routes/1
        [HttpGet("{id:int}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetRoute(int id)
        {
            // Find the route using its ID.
            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteID == id);

            // Return 404 when the route does not exist.
            if (route == null)
            {
                return NotFound(new { message = "Route not found." });
            }

            // Return the route.
            return Ok(route);
        }

        // Allows Organisers to create routes for their own events.
        // POST: /api/Routes
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateRoute(RouteRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Confirm that the event belongs to this Organiser.
            bool ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventID == request.EventID &&
                e.OrganiserID == organiserId);

            if (!ownsEvent)
            {
                return NotFound(new { message = "Event not found for this Organiser." });
            }

            // Check whether this event already has a route.
            bool routeExists = await _context.Routes.AnyAsync(r =>
                r.EventID == request.EventID);

            if (routeExists)
            {
                return Conflict(new { message = "This event already has a route." });
            }

            // Create the route.
            var route = new RaceDayRoute
            {
                EventID = request.EventID,
                RouteName = request.RouteName,
                Distance = request.Distance,
                RouteDescription = request.RouteDescription
            };

            // Add the route to the database.
            _context.Routes.Add(route);

            // Save the new route.
            await _context.SaveChangesAsync();

            // Return the created route.
            return CreatedAtAction(
                nameof(GetRoute),
                new { id = route.RouteID },
                route);
        }

        // Allows Organisers to update routes for their own events.
        // PUT: /api/Routes/1
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateRoute(
            int id, RouteRequest request)
        {
            // Retrieve the logged-in Organiser's ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find the route and confirm that its event belongs to this Organiser.
            var route = await _context.Routes
                .FirstOrDefaultAsync(r =>
                    r.RouteID == id &&
                    r.Event!.OrganiserID == organiserId);

            if (route == null)
            {
                return NotFound(new { message = "Route not found for this Organiser." });
            }

            // Confirm that the destination event also belongs to this Organiser.
            bool ownsEvent = await _context.Events.AnyAsync(e =>
                e.EventID == request.EventID &&
                e.OrganiserID == organiserId);

            if (!ownsEvent)
            {
                return NotFound(new { message = "Event not found for this Organiser." });
            }

            // Prevent another route from already being linked to the destination event.
            bool duplicateRoute = await _context.Routes.AnyAsync(r =>
                r.EventID == request.EventID &&
                r.RouteID != id);

            if (duplicateRoute)
            {
                return Conflict(new { message = "This event already has a route." });
            }

            // Update the route.
            route.EventID = request.EventID;
            route.RouteName = request.RouteName;
            route.Distance = request.Distance;
            route.RouteDescription = request.RouteDescription;

            // Save the changes.
            await _context.SaveChangesAsync();

            // Return the updated route.
            return Ok(route);
        }

        // Allows Organisers to delete routes for their own events.
        // DELETE: /api/Routes/1
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            // Retrieve the logged-in Organiser's ID.
            int? organiserId = HttpContext.Session.GetInt32("UserID");

            // Find the route and confirm event ownership.
            var route = await _context.Routes
                .FirstOrDefaultAsync(r =>
                    r.RouteID == id &&
                    r.Event!.OrganiserID == organiserId);

            if (route == null)
            {
                return NotFound(new { message = "Route not found." });
            }

            // Remove the route.
            _context.Routes.Remove(route);

            // Save the deletion.
            await _context.SaveChangesAsync();

            // Confirm deletion.
            return Ok(new { message = "Route deleted successfully." });
        }
    }
}