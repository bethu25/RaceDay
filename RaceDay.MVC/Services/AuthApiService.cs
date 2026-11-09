using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles login and registration communication with the RaceDay API.
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Logs a user into the RaceDay API.
        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                // Send the login details to the API.
                using HttpResponseMessage response =
                    await _httpClient.PostAsJsonAsync(
                        "api/Auth/login",
                        new { email, password });

                string responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    return false;
                }

                // Read the API session cookie returned after successful login.
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    string? sessionCookie = cookies.FirstOrDefault(cookie =>
                        cookie.StartsWith(
                            ".AspNetCore.Session=",
                            StringComparison.OrdinalIgnoreCase));

                    if (sessionCookie == null)
                    {
                        return false;
                    }

                    // Store the API cookie in the MVC session.
                    string cookieValue = sessionCookie.Split(';')[0];

                    httpContext.Session.SetString(
                        "ApiSessionCookie",
                        cookieValue);
                }
                else
                {
                    return false;
                }

                // Read the user information returned by the API.
                using JsonDocument document =
                    JsonDocument.Parse(responseContent);

                JsonElement user = document.RootElement;

                httpContext.Session.SetInt32(
                    "UserID",
                    user.GetProperty("userID").GetInt32());

                httpContext.Session.SetString(
                    "UserRole",
                    user.GetProperty("role").GetString() ?? "");

                httpContext.Session.SetString(
                    "FullName",
                    user.GetProperty("fullName").GetString() ?? "");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login error: " + ex.Message);
                return false;
            }
        }

        // Registers a new RaceDay user through the API.
        public async Task<string?> RegisterAsync(
            string fullName,
            string email,
            string password,
            string role,
            string phoneNumber)
        {
            try
            {
                // Send the registration details to the API.
                using HttpResponseMessage response =
                    await _httpClient.PostAsJsonAsync(
                        "api/Auth/register",
                        new
                        {
                            fullName,
                            email,
                            password,
                            role,
                            phoneNumber
                        });

                // A successful registration returns HTTP 201.
                if (response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Read the error returned by the API.
                string errorMessage =
                    await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return "An account with this email address already exists.";
                }

                return "Registration failed. " + errorMessage;
            }
            catch (Exception ex)
            {
                return "Could not communicate with the API: " + ex.Message;
            }
        }
    }
}