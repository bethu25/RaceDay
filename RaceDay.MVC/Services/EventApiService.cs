using System.Text.Json;
using System.Net.Http.Json;

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
            JsonElement? categories = await SendGetRequestAsync("api/Categories");

            if (categories == null)
            {
                return null;
            }

            List<JsonElement> eventCategories = new List<JsonElement>();

            if (categories.Value.ValueKind != JsonValueKind.Array)
            {
                LastError = "The categories response has an unexpected format.";
                return null;
            }

            foreach (JsonElement category in categories.Value.EnumerateArray())
            {
                if (category.TryGetProperty("eventID", out JsonElement categoryEventId)
                    && categoryEventId.GetInt32() == eventId)
                {
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

                if (httpContext == null)
                {
                    LastError = "The current MVC session could not be accessed.";
                    return null;
                }

                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
                }

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

                using JsonDocument document = JsonDocument.Parse(json);

                return document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                LastError = "Could not communicate with the API: " + ex.Message;
                return null;
            }
        }

        // Creates the event information sent to the API.
        private object CreateEventData(
            string eventName,
            DateTime eventDate,
            string location,
            string? description,
            decimal distance,
            string eventType)
        {
            return new
            {
                EventName = eventName,
                EventDate = eventDate,
                Location = location,
                Description = description,
                Distance = distance,
                EventType = eventType
            };
        }

        // Sends a POST request to create an event.
        public async Task<bool> CreateEventAsync(
            string eventName,
            DateTime eventDate,
            string location,
            string? description,
            decimal distance,
            string eventType)
        {
            return await SendEventRequestAsync(
                HttpMethod.Post,
                "api/Events",
                CreateEventData(eventName, eventDate, location,
                    description, distance, eventType));
        }

        // Sends a PUT request to update an existing event.
        public async Task<bool> UpdateEventAsync(
            int eventId,
            string eventName,
            DateTime eventDate,
            string location,
            string? description,
            decimal distance,
            string eventType)
        {
            return await SendEventRequestAsync(
                HttpMethod.Put,
                $"api/Events/{eventId}",
                CreateEventData(eventName, eventDate, location,
                    description, distance, eventType));
        }

        // Sends a DELETE request to remove an event.
        public async Task<bool> DeleteEventAsync(int eventId)
        {
            return await SendEventRequestAsync(
                HttpMethod.Delete,
                $"api/Events/{eventId}");
        }

        // Sends authenticated POST, PUT or DELETE requests to the API.
        private async Task<bool> SendEventRequestAsync(
            HttpMethod method,
            string endpoint,
            object? eventData = null)
        {
            LastError = null;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Checks whether the current MVC session is available.
                if (httpContext == null)
                {
                    LastError = "The current MVC session could not be accessed.";
                    return false;
                }

                // Retrieves the API authentication cookie.
                string? apiCookie =
                    httpContext.Session.GetString("ApiSessionCookie");

                // Creates the HTTP request.
                using HttpRequestMessage request =
                    new HttpRequestMessage(method, endpoint);

                // Adds event information for POST and PUT requests.
                if (eventData != null)
                {
                    request.Content = JsonContent.Create(eventData);
                }

                // Forwards the API session cookie.
                if (!string.IsNullOrWhiteSpace(apiCookie))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);
                }

                // Sends the request to the API.
                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                // Handles unsuccessful API responses.
                if (!response.IsSuccessStatusCode)
                {
                    string responseBody =
                        await response.Content.ReadAsStringAsync();

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
    }
}