using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    // Handles requests related to RaceDay events.
    public class EventsController : Controller
    {
        // Stores the service used to communicate with the API.
        private readonly EventApiService _eventApiService;

        // Receives the API service through dependency injection.
        public EventsController(EventApiService eventApiService)
        {
            _eventApiService = eventApiService;
        }

        // Displays events retrieved from the API.
        public async Task<IActionResult> Index()
        {
            // Request events from the API.
            var events = await _eventApiService.GetEventsAsync();

            // Check whether the API request failed.
            if (events == null)
            {
                // Display the actual error on the website.
                ViewBag.ErrorMessage = _eventApiService.LastError;

                return View();
            }

            // Send the events to the Razor view.
            return View(events);
        }
    }
}