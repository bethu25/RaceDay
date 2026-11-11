using System.Text.Json;
using System.Net.Http.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication with the API for participant results.
    public class ResultApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores API errors so the MVC page can display them.
        public string? LastError { get; private set; }

        public ResultApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Gets results belonging to the logged-in user.
        public async Task<JsonElement> GetMyResultsAsync()
        {
            LastError = null;

            // Retrieve the API session cookie saved during login.
            string? apiCookie = _httpContextAccessor.HttpContext?
                .Session.GetString("ApiSessionCookie");

            // Create a GET request to retrieve results.
            var request = new HttpRequestMessage(
                HttpMethod.Get, "api/Results");

            // Forward the API session cookie for authentication.
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }

            // Send the request to the API.
            var response = await _httpClient.SendAsync(request);

            // Handle unsuccessful API responses.
            if (!response.IsSuccessStatusCode)
            {
                LastError = $"API Error: {(int)response.StatusCode} " +
                            $"{response.ReasonPhrase}";

                return default;
            }

            // Read and return the results as JSON.
            using var document = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            return document.RootElement.Clone();
        }

        // Records a participant's finishing time and position.
        public async Task<bool> RecordResultAsync(
            int enrolmentId,
            TimeSpan finishTime,
            int position)
        {
            LastError = null;

            // Retrieve the API session cookie.
            string? apiCookie = _httpContextAccessor.HttpContext?
                .Session.GetString("ApiSessionCookie");

            // Create a POST request to record the result.
            var request = new HttpRequestMessage(
                HttpMethod.Post, "api/Results");

            // Forward the API session cookie.
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }

            // Prepare the result information.
            request.Content = JsonContent.Create(new
            {
                enrolmentID = enrolmentId,
                finishTime = finishTime,
                position = position
            });

            // Send the result to the API.
            var response = await _httpClient.SendAsync(request);

            // Handle unsuccessful responses.
            if (!response.IsSuccessStatusCode)
            {
                string errorResponse =
                    await response.Content.ReadAsStringAsync();

                LastError = $"API Error: {(int)response.StatusCode}. " +
                            errorResponse;

                return false;
            }

            // Confirm that the result was recorded successfully.
            return true;
        }
    }
}