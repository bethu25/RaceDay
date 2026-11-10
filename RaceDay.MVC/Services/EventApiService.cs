using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication between the MVC application and the RaceDay API.
    public class EventApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores API errors so that controllers can display them.
        public string? LastError { get; private set; }

        public EventApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Retrieves all events from the API.
        public async Task<JsonElement?> GetEventsAsync()
        {
            return await SendGetRequestAsync("api/Events");
        }

        // Retrieves one event using its event ID.
        public async Task<JsonElement?> GetEventAsync(int eventId)
        {
            return await SendGetRequestAsync($"api/Events/{eventId}");
        }

        // Retrieves categories belonging to a specific event.
        public async Task<List<JsonElement>?> GetCategoriesForEventAsync(int eventId)
        {
            // Retrieve all categories from the API.
            JsonElement? categories = await SendGetRequestAsync("api/Categories");

            // Stop if the API request failed.
            if (categories == null)
            {
                return null;
            }

            // Store categories belonging to the requested event.
            List<JsonElement> eventCategories = new List<JsonElement>();

            // Make sure the API returned an array.
            if (categories.Value.ValueKind != JsonValueKind.Array)
            {
                LastError = "The categories response has an unexpected format.";
                return null;
            }

            // Check each category.
            foreach (JsonElement category in categories.Value.EnumerateArray())
            {
                // Check whether the category belongs to this event.
                if (category.TryGetProperty("eventID", out JsonElement categoryEventId)
                    && categoryEventId.GetInt32() == eventId)
                {
                    // Clone the category so it remains available after JSON processing.
                    eventCategories.Add(category.Clone());
                }
            }

            return eventCategories;
        }

        // Sends an authenticated GET request to the API.
        private async Task<JsonElement?> SendGetRequestAsync(string endpoint)
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Check whether the current MVC session is available.
                if (httpContext == null)
                {
                    LastError = "The current MVC session could not be accessed.";
                    return null;
                }

                // Retrieve the API session cookie saved during login.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Get, endpoint);

                // Forward the API session cookie for authentication.
                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                // Send the request to the API.
                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string json =
                    await response.Content.ReadAsStringAsync();

                // Handle unsuccessful API responses.
                if (!response.IsSuccessStatusCode)
                {
                    LastError =
                        $"API Error: {(int)response.StatusCode} " +
                        $"{response.ReasonPhrase}. Response: {json}";

                    return null;
                }

                // Convert the JSON response into a JsonElement.
                using JsonDocument document = JsonDocument.Parse(json);

                return document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                // Store the error instead of allowing the application to crash.
                LastError = "Could not communicate with the API: " + ex.Message;

                return null;
            }
        }
    }
}