using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    // Handles profile information and profile picture uploads.
    public class ProfileController : Controller
    {
        private readonly ProfileApiService _profileApiService;

        // Receives the service that communicates with the API.
        public ProfileController(ProfileApiService profileApiService)
        {
            _profileApiService = profileApiService;
        }

        // Checks whether the user is logged in.
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID").HasValue;
        }

        // Converts the API profile response into a view model.
        private ProfileViewModel CreateProfileModel(JsonElement profile)
        {
            return new ProfileViewModel
            {
                UserID = profile.GetProperty("userID").GetInt32(),
                FullName = profile.GetProperty("fullName").GetString() ?? "",
                Email = profile.GetProperty("email").GetString() ?? "",
                Role = profile.GetProperty("role").GetString() ?? "",

                PhoneNumber = profile.TryGetProperty("phoneNumber", out var phone)
                    ? phone.GetString()
                    : null,

                ProfileImage = profile.TryGetProperty("profileImage", out var image)
                    ? image.GetString()
                    : null
            };
        }

        // Displays the logged-in user's profile.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            JsonElement? profile = await _profileApiService.GetProfileAsync();

            if (profile == null)
            {
                ViewBag.ErrorMessage = "Unable to load your profile.";
                return View(new ProfileViewModel());
            }

            var model = CreateProfileModel(profile.Value);

            return View(model);
        }

        // Displays the profile editing form.
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            JsonElement? profile = await _profileApiService.GetProfileAsync();

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Unable to load your profile.";
                return RedirectToAction("Index");
            }

            var model = CreateProfileModel(profile.Value);

            return View(model);
        }

        // Saves the updated profile information.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.ErrorMessage = "Full name and email are required.";
                return View(model);
            }

            // Send the updated information to the API.
            bool updated = await _profileApiService.UpdateProfileAsync(
                model.FullName,
                model.Email,
                model.PhoneNumber);

            if (updated)
            {
                // Update the displayed name in the MVC session.
                HttpContext.Session.SetString("FullName", model.FullName);

                TempData["SuccessMessage"] =
                    "Your profile was updated successfully.";

                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage =
                "Unable to update your profile. " + _profileApiService.LastError;

            return View(model);
        }

        // Uploads a new profile picture through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction("Index");
            }

            bool uploaded =
                await _profileApiService.UploadProfilePictureAsync(file);

            if (uploaded)
            {
                TempData["SuccessMessage"] =
                    "Your profile picture was uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Picture upload failed. " + _profileApiService.LastError;
            }

            return RedirectToAction("Index");
        }

        // Displays the profile picture retrieved through the API.
        [HttpGet]
        public async Task<IActionResult> Picture()
        {
            if (!IsLoggedIn())
            {
                return Unauthorized();
            }

            var picture = await _profileApiService.GetProfilePictureAsync();

            if (picture == null)
            {
                return NotFound();
            }

            return File(picture.Value.Content, picture.Value.ContentType);
        }
    }
}