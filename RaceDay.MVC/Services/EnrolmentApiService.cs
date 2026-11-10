using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication with the API for participant enrolments.
    public class EnrolmentApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores API errors so the controller can display them.
        public string? LastError { get; private set; }

        public EnrolmentApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Sends an enrolment request to the API.
        public async Task<bool> EnrolAsync(int categoryId)
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    LastError = "The current session could not be accessed.";
                    return false;
                }

                // Retrieve the API session cookie saved during login.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                // Prepare the enrolment request.
                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Post, "api/Enrolments");

                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                // Send the selected category ID to the API.
                request.Content = JsonContent.Create(new
                {
                    categoryID = categoryId
                });

                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LastError =
                        $"API Error: {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}. Response: {responseBody}";

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError =
                    "Could not communicate with the API: " + ex.Message;

                return false;
            }
        }

        // Retrieves enrolments belonging to the logged-in participant.
        public async Task<JsonElement?> GetMyEnrolmentsAsync()
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    LastError = "The current session could not be accessed.";
                    return null;
                }

                // Retrieve the API session cookie saved during login.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                // Prepare a GET request to retrieve participant enrolments.
                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Get, "api/Enrolments");

                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                // Send the request to the API.
                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LastError =
                        $"API Error: {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}. Response: {json}";

                    return null;
                }

                // Convert the API response into JSON.
                using JsonDocument document = JsonDocument.Parse(json);

                return document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                LastError =
                    "Could not communicate with the API: " + ex.Message;

                return null;
            }
        }

        // Cancels an enrolment through the API.
        public async Task<bool> CancelEnrolmentAsync(int enrolmentId)
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    LastError = "The current session could not be accessed.";
                    return false;
                }

                // Retrieve the API session cookie.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                // Prepare a DELETE request for the selected enrolment.
                using HttpRequestMessage request =
                    new HttpRequestMessage(
                        HttpMethod.Delete,
                        $"api/Enrolments/{enrolmentId}");

                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                // Send the cancellation request to the API.
                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LastError =
                        $"API Error: {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}. Response: {responseBody}";

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = "Could not communicate with the API: " + ex.Message;
                return false;
            }
        }
    }
}