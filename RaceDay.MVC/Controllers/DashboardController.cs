using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles the organiser dashboard.
    public class DashboardController : Controller
    {
        private readonly EventApiService _eventApiService;
        private readonly EnrolmentApiService _enrolmentApiService;

        // Receives the existing API services through dependency injection.
        public DashboardController(
            EventApiService eventApiService,
            EnrolmentApiService enrolmentApiService)
        {
            _eventApiService = eventApiService;
            _enrolmentApiService = enrolmentApiService;
        }

        // Checks whether the logged-in user is an organiser.
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // Displays the organiser dashboard.
        public async Task<IActionResult> Index()
        {
            // Redirect users who are not logged in.
            if (!HttpContext.Session.GetInt32("UserID").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Prevent participants from accessing the dashboard.
            if (!IsOrganiser())
            {
                return Forbid();
            }

            // Retrieve the logged-in organiser's ID.
            int organiserId =
                HttpContext.Session.GetInt32("UserID")!.Value;

            // Retrieve all events through the API.
            var eventsResponse =
                await _eventApiService.GetEventsAsync();

            if (eventsResponse == null)
            {
                ViewBag.ErrorMessage = _eventApiService.LastError;
                return View(new DashboardViewModel());
            }

            // Retrieve enrolments belonging to this organiser's events.
            var enrolmentsResponse =
                await _enrolmentApiService.GetMyEnrolmentsAsync();

            if (enrolmentsResponse == null)
            {
                ViewBag.ErrorMessage = _enrolmentApiService.LastError;
                return View(new DashboardViewModel());
            }

            // Prepare the dashboard information.
            DashboardViewModel model = new DashboardViewModel();

            // Check that the events response contains an array.
            if (eventsResponse.Value.ValueKind != JsonValueKind.Array)
            {
                ViewBag.ErrorMessage = "The events response has an unexpected format.";
                return View(model);
            }

            // Check that the enrolments response contains an array.
            if (enrolmentsResponse.Value.ValueKind != JsonValueKind.Array)
            {
                ViewBag.ErrorMessage = "The enrolments response has an unexpected format.";
                return View(model);
            }

            // Get the events belonging to the logged-in organiser.
            var organiserEvents = eventsResponse.Value
                .EnumerateArray()
                .Where(e =>
                    e.TryGetProperty("organiserID", out JsonElement id)
                    && id.GetInt32() == organiserId)
                .ToList();

            // Retrieve the organiser's enrolments.
            var enrolments = enrolmentsResponse.Value
                .EnumerateArray()
                .ToList();

            // Calculate the dashboard summary figures.
            model.TotalEvents = organiserEvents.Count;
            model.TotalEnrolments = enrolments.Count;

            // Count events scheduled for future dates.
            model.UpcomingEvents = organiserEvents.Count(e =>
                e.GetProperty("eventDate").GetDateTime() > DateTime.Today);

            // Prepare the event overview.
            foreach (JsonElement eventItem in organiserEvents)
            {
                int eventId = eventItem.GetProperty("eventID").GetInt32();

                // Count enrolments belonging to this event.
                int eventEnrolments = enrolments.Count(e =>
                    e.TryGetProperty("eventID", out JsonElement id)
                    && id.GetInt32() == eventId);

                // Add the event information to the dashboard.
                model.Events.Add(new DashboardEventViewModel
                {
                    EventID = eventId,
                    EventName = eventItem.GetProperty("eventName").GetString() ?? "",
                    EventDate = eventItem.GetProperty("eventDate").GetDateTime(),
                    Location = eventItem.GetProperty("location").GetString() ?? "",
                    TotalEnrolments = eventEnrolments
                });
            }

            // Display upcoming events first.
            model.Events = model.Events
                .OrderBy(e => e.EventDate)
                .ToList();

            return View(model);
        }
    }
}