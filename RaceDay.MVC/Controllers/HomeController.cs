using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Diagnostics;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles the RaceDay homepage.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventApiService _eventApiService;

        // Connects the homepage to the Events API service.
        public HomeController(
            ILogger<HomeController> logger,
            EventApiService eventApiService)
        {
            _logger = logger;
            _eventApiService = eventApiService;
        }

        // Displays the homepage and retrieves upcoming events.
        public async Task<IActionResult> Index()
        {
            // Retrieves events from the RaceDay API.
            JsonElement? events =
                await _eventApiService.GetEventsAsync();

            // Creates a list for the homepage events.
            List<JsonElement> upcomingEvents = new List<JsonElement>();

            // Checks whether the API returned an event list.
            if (events.HasValue &&
                events.Value.ValueKind == JsonValueKind.Array)
            {
                // Displays a maximum of three events.
                foreach (JsonElement raceEvent in
                    events.Value.EnumerateArray().Take(3))
                {
                    upcomingEvents.Add(raceEvent.Clone());
                }
            }
            else
            {
                // Stores an error if events cannot be retrieved.
                ViewBag.EventError = _eventApiService.LastError;
            }

            // Sends the events to the homepage.
            ViewBag.UpcomingEvents = upcomingEvents;

            return View();
        }

        // Displays the privacy page.
        public IActionResult Privacy()
        {
            return View();
        }

        // Handles application errors.
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}