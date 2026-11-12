using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles organiser category management.
    public class CategoriesController : Controller
    {
        private readonly CategoryApiService _categoryApiService;
        private readonly EventApiService _eventApiService;

        public CategoriesController(
            CategoryApiService categoryApiService,
            EventApiService eventApiService)
        {
            _categoryApiService = categoryApiService;
            _eventApiService = eventApiService;
        }

        // Checks whether the current user is logged in.
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID").HasValue;
        }

        // Checks whether the current user is an organiser.
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        // Displays categories belonging to a selected event.
        public async Task<IActionResult> Index(int eventId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            var categories =
                await _categoryApiService.GetCategoriesForEventAsync(eventId);

            if (categories == null)
            {
                ViewBag.ErrorMessage = _categoryApiService.LastError;
                categories = new List<JsonElement>();
            }

            ViewBag.EventID = eventId;

            return View(categories);
        }

        // Displays the form for creating a category.
        [HttpGet]
        public IActionResult Create(int eventId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            return View(new CategoryViewModel
            {
                EventID = eventId
            });
        }

        // Creates a category through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            bool created = await _categoryApiService.CreateCategoryAsync(
                model.EventID,
                model.CategoryName,
                model.Distance,
                model.EntryFee,
                model.MaxParticipants);

            if (!created)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _categoryApiService.LastError ?? "Category creation failed.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Category created successfully!";

            return RedirectToAction("Index",
                new { eventId = model.EventID });
        }

        // Displays an existing category for editing.
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int eventId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            var categories = await _categoryApiService.GetCategoriesAsync();

            if (categories == null ||
                categories.Value.ValueKind != JsonValueKind.Array)
            {
                TempData["ErrorMessage"] = _categoryApiService.LastError;
                return RedirectToAction("Index", new { eventId });
            }

            foreach (JsonElement category in categories.Value.EnumerateArray())
            {
                if (category.GetProperty("categoryID").GetInt32() == id)
                {
                    return View(new CategoryViewModel
                    {
                        CategoryID = id,
                        EventID = category.GetProperty("eventID").GetInt32(),
                        CategoryName = category.GetProperty("categoryName").GetString() ?? "",
                        Distance = category.GetProperty("distance").GetDecimal(),
                        EntryFee = category.GetProperty("entryFee").GetDecimal(),
                        MaxParticipants = category.GetProperty("maxParticipants").GetInt32()
                    });
                }
            }

            TempData["ErrorMessage"] = "Category could not be found.";

            return RedirectToAction("Index", new { eventId });
        }

        // Saves changes to an existing category.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            bool updated = await _categoryApiService.UpdateCategoryAsync(
                model.CategoryID,
                model.EventID,
                model.CategoryName,
                model.Distance,
                model.EntryFee,
                model.MaxParticipants);

            if (!updated)
            {
                ModelState.AddModelError(
                    string.Empty,
                    _categoryApiService.LastError ?? "Category update failed.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Category updated successfully!";

            return RedirectToAction("Index",
                new { eventId = model.EventID });
        }

        // Deletes a category through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int eventId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsOrganiser())
                return Forbid();

            bool deleted = await _categoryApiService.DeleteCategoryAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] = _categoryApiService.LastError
                    ?? "Category deletion failed.";
            }
            else
            {
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }

            return RedirectToAction("Index", new { eventId });
        }
    }
}