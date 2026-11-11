using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles requests related to RaceDay events.
    public class EventsController : Controller
    {
        private readonly EventApiService _eventApiService;

        public EventsController(EventApiService eventApiService)
        {
            _eventApiService = eventApiService;
        }

        // Checks whether the current user is an organiser.
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // Checks whether a valid user ID exists in the current session.
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID").HasValue;
        }

        // Displays all available events.
        public async Task<IActionResult> Index()
        {
            var events = await _eventApiService.GetEventsAsync();

            if (events == null)
            {
                ViewBag.ErrorMessage = _eventApiService.LastError;
                return View();
            }

            return View(events);
        }

        // Displays the details of a selected event.
        public async Task<IActionResult> Details(int id)
        {
            JsonElement? eventDetails =
                await _eventApiService.GetEventAsync(id);

            if (eventDetails == null)
            {
                ViewBag.ErrorMessage = _eventApiService.LastError;
                return View();
            }

            var categories =
                await _eventApiService.GetCategoriesForEventAsync(id);

            ViewBag.Categories = categories ?? new List<JsonElement>();

            if (categories == null)
            {
                ViewBag.CategoryError = _eventApiService.LastError;
            }

            return View(eventDetails.Value);
        }

        // Displays the Create Event form to organisers.
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return Forbid();
            }

            return View(new EventCreateViewModel());
        }

        // Receives and processes the new event form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool created = await _eventApiService.CreateEventAsync(
                model.EventName,
                model.EventDate,
                model.Location,
                model.Description,
                model.Distance,
                model.EventType
            );

            if (!created)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _eventApiService.LastError ?? "The event could not be created.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event created successfully!";

            return RedirectToAction("Index");
        }

        // Displays an existing event so the organiser can edit it.
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return Forbid();
            }

            JsonElement? eventDetails =
                await _eventApiService.GetEventAsync(id);

            if (eventDetails == null)
            {
                TempData["ErrorMessage"] = _eventApiService.LastError;
                return RedirectToAction("Index");
            }

            // Reads the existing event information from the API response.
            JsonElement eventData = eventDetails.Value;

            var model = new EventCreateViewModel
            {
                EventName = eventData.GetProperty("eventName").GetString() ?? "",
                EventDate = eventData.GetProperty("eventDate").GetDateTime(),
                Location = eventData.GetProperty("location").GetString() ?? "",
                Description = eventData.TryGetProperty(
                    "description", out JsonElement description)
                    ? description.GetString()
                    : null,
                Distance = eventData.GetProperty("distance").GetDecimal(),
                EventType = eventData.GetProperty("eventType").GetString() ?? ""
            };

            return View(model);
        }

        // Receives and saves the organiser's event changes.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EventCreateViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool updated = await _eventApiService.UpdateEventAsync(
                id,
                model.EventName,
                model.EventDate,
                model.Location,
                model.Description,
                model.Distance,
                model.EventType
            );

            if (!updated)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _eventApiService.LastError ?? "The event could not be updated.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event updated successfully!";

            return RedirectToAction("Index");
        }

        // Deletes an event after receiving a POST request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!IsOrganiser())
            {
                return Forbid();
            }

            bool deleted = await _eventApiService.DeleteEventAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] = _eventApiService.LastError
                    ?? "The event could not be deleted.";
            }
            else
            {
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
 