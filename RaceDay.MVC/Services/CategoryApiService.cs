using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication between MVC and the Categories API.
    public class CategoryApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores API errors for display in MVC pages.
        public string? LastError { get; private set; }

        public CategoryApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Retrieves all categories from the API.
        public async Task<JsonElement?> GetCategoriesAsync()
        {
            return await SendGetRequestAsync("api/Categories");
        }

        // Retrieves categories belonging to one event.
        public async Task<List<JsonElement>?> GetCategoriesForEventAsync(int eventId)
        {
            JsonElement? categories = await GetCategoriesAsync();

            if (categories == null)
                return null;

            if (categories.Value.ValueKind != JsonValueKind.Array)
            {
                LastError = "Unexpected categories response.";
                return null;
            }

            List<JsonElement> eventCategories = new List<JsonElement>();

            foreach (JsonElement category in categories.Value.EnumerateArray())
            {
                if (category.TryGetProperty("eventID", out JsonElement categoryEventID)
                    && categoryEventID.GetInt32() == eventId)
                {
                    eventCategories.Add(category.Clone());
                }
            }

            return eventCategories;
        }

        // Creates a new category.
        public async Task<bool> CreateCategoryAsync(
            int eventId,
            string categoryName,
            decimal distance,
            decimal entryFee,
            int maxParticipants)
        {
            var categoryData = new
            {
                EventID = eventId,
                CategoryName = categoryName,
                Distance = distance,
                EntryFee = entryFee,
                MaxParticipants = maxParticipants
            };

            return await SendRequestAsync(
                HttpMethod.Post,
                "api/Categories",
                categoryData);
        }

        // Updates an existing category.
        public async Task<bool> UpdateCategoryAsync(
            int categoryId,
            int eventId,
            string categoryName,
            decimal distance,
            decimal entryFee,
            int maxParticipants)
        {
            var categoryData = new
            {
                EventID = eventId,
                CategoryName = categoryName,
                Distance = distance,
                EntryFee = entryFee,
                MaxParticipants = maxParticipants
            };

            return await SendRequestAsync(
                HttpMethod.Put,
                $"api/Categories/{categoryId}",
                categoryData);
        }

        // Deletes an existing category.
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            return await SendRequestAsync(
                HttpMethod.Delete,
                $"api/Categories/{categoryId}");
        }

        // Sends authenticated GET requests.
        private async Task<JsonElement?> SendGetRequestAsync(string endpoint)
        {
            LastError = null;

            try
            {
                var context = _httpContextAccessor.HttpContext;

                if (context == null)
                {
                    LastError = "MVC session is unavailable.";
                    return null;
                }

                string? cookie = context.Session.GetString("ApiSessionCookie");

                using HttpRequestMessage request =
                    new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
                }

                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    LastError = $"API Error: {(int)response.StatusCode}. {json}";
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

        // Sends authenticated POST, PUT and DELETE requests.
        private async Task<bool> SendRequestAsync(
            HttpMethod method,
            string endpoint,
            object? data = null)
        {
            LastError = null;

            try
            {
                var context = _httpContextAccessor.HttpContext;

                if (context == null)
                {
                    LastError = "MVC session is unavailable.";
                    return false;
                }

                string? cookie = context.Session.GetString("ApiSessionCookie");

                using HttpRequestMessage request =
                    new HttpRequestMessage(method, endpoint);

                // Attach category information when required.
                if (data != null)
                {
                    request.Content = JsonContent.Create(data);
                }

                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
                }

                using HttpResponseMessage response =
                    await _httpClient.SendAsync(request);

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
                LastError = "Could not communicate with the API: " + ex.Message;
                return false;
            }
        }
    }
}