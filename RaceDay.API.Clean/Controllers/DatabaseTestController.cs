using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]


    // Defines the URL used to access this controller.
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        // Gives this controller access to the RaceDay database.
        private readonly RaceDayDbContext _context;

        // Receives the database context through dependency injection.
        public DatabaseTestController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Tests whether the API can retrieve users from SQL Server.
        // GET: /api/DatabaseTest/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Retrieve users from the existing Users table.
            var users = await _context.Users.ToListAsync();

            // Return the retrieved users as a JSON response.
            return Ok(users);
        }

        // Tests whether the API can retrieve events from SQL Server.
        // GET: /api/DatabaseTest/events
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            // Retrieve all events from the Events table.
            var events = await _context.Events.ToListAsync();

            // Return the events as a JSON response.
            return Ok(events);
        }

        // Tests whether the API can retrieve categories from SQL Server.
        // GET: /api/DatabaseTest/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            // Retrieve all categories from the Categories table.
            var categories = await _context.Categories.ToListAsync();

            // Return the categories as a JSON response.
            return Ok(categories);
        }

        // Tests whether the API can retrieve enrolments from SQL Server.
        // GET: /api/DatabaseTest/enrolments
        [HttpGet("enrolments")]
        public async Task<IActionResult> GetEnrolments()
        {
            // Retrieve all enrolments from the Enrolments table.
            var enrolments = await _context.Enrolments.ToListAsync();

            // Return the enrolments as a JSON response.
            return Ok(enrolments);
        }

        // Tests whether the API can retrieve results from SQL Server.
        // GET: /api/DatabaseTest/results
        [HttpGet("results")]
        public async Task<IActionResult> GetResults()
        {
            // Retrieve all results from the Results table.
            var results = await _context.Results.ToListAsync();

            // Return the results as a JSON response.
            return Ok(results);
        }

        // Tests whether the API can retrieve routes from SQL Server.
        // GET: /api/DatabaseTest/routes
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes()
        {
            // Retrieve all routes from the Routes table.
            var routes = await _context.Routes.ToListAsync();

            // Return the routes as a JSON response.
            return Ok(routes);
        }
    }

}
