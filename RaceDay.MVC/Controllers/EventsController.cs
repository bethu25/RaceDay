using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using System.Text.Json;

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

        // Displays all events retrieved from the API.
        public async Task<IActionResult> Index()
        {
            // Request events from the API.
            var events = await _eventApiService.GetEventsAsync();

            // Check whether the API request failed.
            if (events == null)
            {
                ViewBag.ErrorMessage = _eventApiService.LastError;
                return View();
            }

            // Send the events to the Razor view.
            return View(events);
        }

        // Displays the full details of one event.
        // Example URL: /Events/Details/1
        public async Task<IActionResult> Details(int id)
        {
            // Retrieve the selected event from the API.
            JsonElement? eventDetails =
                await _eventApiService.GetEventAsync(id);

            // Check whether the event could be retrieved.
            if (eventDetails == null)
            {
                ViewBag.ErrorMessage = _eventApiService.LastError;
                return View();
            }

            // Retrieve categories belonging to this event.
            var categories =
                await _eventApiService.GetCategoriesForEventAsync(id);

            // Store categories so that the Details view can display them.
            ViewBag.Categories = categories ?? new List<JsonElement>();

            // Display an error if the categories could not be retrieved.
            if (categories == null)
            {
                ViewBag.CategoryError = _eventApiService.LastError;
            }

            // Send the event details to the Details view.
            return View(eventDetails.Value);
        }
    }
}