using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Services;

namespace RaceDay.API.Clean.Controllers
{
    // Identifies this class as an API controller.
    [ApiController]

    // Sets the base URL for authentication endpoints.
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Provides access to registration and login functionality.
        private readonly AuthService _authService;

        // Receives the authentication service through dependency injection.
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Registers a new user.
        // POST: /api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Register the user and securely hash their password.
            var user = await _authService.RegisterAsync(request);

            // Return a conflict response if the email is already registered.
            if (user == null)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            // Return the account details without exposing the password hash.
            return Created("", new
            {
                user.UserID,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber
            });
        }

        // Logs an existing user into RaceDay.
        // POST: /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Check the submitted email and password using the authentication service.
            var user = await _authService.LoginAsync(request);

            // Reject the login if the email or password is incorrect.
            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email address or password."
                });
            }

            // Store the authenticated user's ID in their session.
            HttpContext.Session.SetInt32("UserID", user.UserID);

            // Store the user's role to support role-based access checks.
            HttpContext.Session.SetString("UserRole", user.Role);

            // Store the user's name for use by the application.
            HttpContext.Session.SetString("FullName", user.FullName);

            // Return account details without exposing the password hash.
            return Ok(new
            {
                message = "Login successful.",
                user.UserID,
                user.FullName,
                user.Email,
                user.Role
            });
        }

    }


}
