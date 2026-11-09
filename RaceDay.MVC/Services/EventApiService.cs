using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication between the MVC application and the RaceDay API.
    public class EventApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores API errors so that the controller can display them.
        public string? LastError { get; private set; }

        public EventApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Retrieves events from the API using the logged-in user's API session.
        public async Task<JsonElement?> GetEventsAsync()
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    LastError = "The current MVC session could not be accessed.";
                    return null;
                }

                // Retrieve the API session cookie saved during login.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Get, "api/Events");

                // Forward the API session cookie to authenticate the request.
                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string json =
                    await response.Content.ReadAsStringAsync();

                // Save the error if the API request was unsuccessful.
                if (!response.IsSuccessStatusCode)
                {
                    LastError =
                        $"API Error: {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}. Response: {json}";

                    return null;
                }

                // Convert the successful JSON response into a JsonElement.
                using JsonDocument document = JsonDocument.Parse(json);

                return document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                LastError = "Could not communicate with the API: " + ex.Message;
                return null;
            }
        }
    }
}