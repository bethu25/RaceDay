using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles participant enrolment requests and enrolment history.
    public class EnrolmentsController : Controller
    {
        private readonly EnrolmentApiService _enrolmentApiService;

        public EnrolmentsController(
            EnrolmentApiService enrolmentApiService)
        {
            _enrolmentApiService = enrolmentApiService;
        }

        // Displays the logged-in participant's enrolments.
        public async Task<IActionResult> MyEnrolments()
        {
            // Check whether the user is logged in.
            if (HttpContext.Session.GetString("UserID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only participants can access this page.
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            // Retrieve the participant's enrolments from the API.
            JsonElement? enrolments =
                await _enrolmentApiService.GetMyEnrolmentsAsync();

            if (enrolments == null)
            {
                ViewBag.ErrorMessage = _enrolmentApiService.LastError;
                return View();
            }

            // Pass the enrolment information to the view.
            return View(enrolments.Value);
        }

        // Submits a participant's enrolment for a selected category.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId, int eventId)
        {
            // Check whether the user is logged in.
            if (HttpContext.Session.GetString("UserID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only participants can enrol in events.
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Participant")
            {
                TempData["EnrolmentError"] =
                    "Only participants can enrol in events.";

                return RedirectToAction("Details", "Events",
                    new { id = eventId });
            }

            // Send the enrolment request to the API.
            bool success =
                await _enrolmentApiService.EnrolAsync(categoryId);

            if (success)
            {
                TempData["EnrolmentSuccess"] =
                    "You have successfully enrolled in this category!";
            }
            else
            {
                TempData["EnrolmentError"] =
                    _enrolmentApiService.LastError ??
                    "Your enrolment could not be completed.";
            }

            return RedirectToAction("Details", "Events",
                new { id = eventId });
        }

        // Cancels an enrolment selected by the participant.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int enrolmentId)
        {
            // Check whether the user is logged in.
            if (HttpContext.Session.GetString("UserID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only participants can cancel enrolments.
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            // Send the cancellation request to the API.
            bool success =
                await _enrolmentApiService.CancelEnrolmentAsync(enrolmentId);

            if (success)
            {
                TempData["CancellationSuccess"] =
                    "Your enrolment has been cancelled successfully.";
            }
            else
            {
                TempData["CancellationError"] =
                    _enrolmentApiService.LastError ??
                    "Your enrolment could not be cancelled.";
            }

            // Return to the My Enrolments page.
            return RedirectToAction("MyEnrolments");
        }
    }
}