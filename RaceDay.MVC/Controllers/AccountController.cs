using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    // Handles user registration, login and logout.
    public class AccountController : Controller
    {
        private readonly AuthApiService _authApiService;

        public AccountController(AuthApiService authApiService)
        {
            _authApiService = authApiService;
        }

        // Displays the login page.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Processes login details.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            bool loginSuccessful =
                await _authApiService.LoginAsync(email, password);

            if (!loginSuccessful)
            {
                ViewBag.ErrorMessage =
                    "Login failed. Please check your details and try again.";

                return View();
            }

            // Redirect the user to the events page after login.
            return RedirectToAction("Index", "Events");
        }

        // Displays the registration page.
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Processes new user registration.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string fullName,
            string email,
            string password,
            string role,
            string phoneNumber)
        {
            // Send registration details to the API.
            string? errorMessage = await _authApiService.RegisterAsync(
                fullName,
                email,
                password,
                role,
                phoneNumber);

            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View();
            }

            // Registration succeeded. Send the user to login.
            TempData["SuccessMessage"] =
                "Your RaceDay account was created successfully. Please log in.";

            return RedirectToAction("Login");
        }

        // Logs the user out of the MVC website.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the MVC session and saved API session cookie.
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}