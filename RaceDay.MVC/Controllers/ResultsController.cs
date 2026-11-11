using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    // Handles participant and organiser result pages.
    public class ResultsController : Controller
    {
        private readonly ResultApiService _resultApiService;
        private readonly EnrolmentApiService _enrolmentApiService;

        // Receives the required API services.
        public ResultsController(
            ResultApiService resultApiService,
            EnrolmentApiService enrolmentApiService)
        {
            _resultApiService = resultApiService;
            _enrolmentApiService = enrolmentApiService;
        }

        // Displays results belonging to the logged-in participant.
        public async Task<IActionResult> MyResults()
        {
            // Check whether the user is logged in.
            string? userID = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrWhiteSpace(userID))
            {
                return RedirectToAction("Login", "Account");
            }

            // Only participants may access this page.
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            // Request the participant's results from the API.
            var results = await _resultApiService.GetMyResultsAsync();

            // Display an error if the API request failed.
            if (_resultApiService.LastError != null)
            {
                ViewBag.ErrorMessage = _resultApiService.LastError;
            }

            return View(results);
        }

        // Displays enrolments belonging to the Organiser's events.
        public async Task<IActionResult> Manage()
        {
            // Check whether the user is logged in.
            string? userID = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrWhiteSpace(userID))
            {
                return RedirectToAction("Login", "Account");
            }

            // Only Organisers may manage results.
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Organiser")
            {
                return RedirectToAction("Index", "Home");
            }

            // Retrieve enrolments from the API.
            var enrolments =
                await _enrolmentApiService.GetMyEnrolmentsAsync();

            // Display API errors if any occur.
            if (_enrolmentApiService.LastError != null)
            {
                ViewBag.ErrorMessage =
                    _enrolmentApiService.LastError;
            }

            // Send the enrolments to the Organiser's view.
            return View(enrolments);
        }

        // Records a participant's result.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(
            int enrolmentId,
            string finishTime,
            int position)
        {
            // Only Organisers may record results.
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Index", "Home");
            }

            // Validate the finishing time.
            if (!TimeSpan.TryParse(finishTime, out TimeSpan parsedTime)
                || parsedTime < TimeSpan.Zero)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid finishing time.";

                return RedirectToAction(nameof(Manage));
            }

            // Validate the finishing position.
            if (position < 1)
            {
                TempData["ErrorMessage"] =
                    "Finishing position must be at least 1.";

                return RedirectToAction(nameof(Manage));
            }

            // Send the result to the API.
            bool success = await _resultApiService.RecordResultAsync(
                enrolmentId,
                parsedTime,
                position);

            // Display the appropriate message.
            if (success)
            {
                TempData["SuccessMessage"] =
                    "Participant result recorded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    _resultApiService.LastError;
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}