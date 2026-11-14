using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    // Handles communication with the API for user profiles and pictures.
    public class ProfileApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Stores error messages returned by the API.
        public string? LastError { get; private set; }

        // Receives the HTTP client and current user's session.
        public ProfileApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        // Gets the current user's profile from the API.
        public async Task<JsonElement?> GetProfileAsync()
        {
            LastError = null;

            // Create a request to retrieve the profile.
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "api/Profile");

            // Forward the user's API session cookie.
            AddSessionCookie(request);

            // Send the request to the API.
            using var response = await _httpClient.SendAsync(request);

            // Check whether the request succeeded.
            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return null;
            }

            // Read the returned profile information.
            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }

        // Uploads a profile picture through the API to Azure.
        public async Task<bool> UploadProfilePictureAsync(IFormFile file)
        {
            LastError = null;

            // Create a multipart request for the image.
            using var content = new MultipartFormDataContent();

            // Read the selected image.
            using var fileStream = file.OpenReadStream();

            // Prepare the image content.
            using var imageContent = new StreamContent(fileStream);

            // Set the image content type.
            imageContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            // Add the image using the API's expected parameter name.
            content.Add(imageContent, "file", file.FileName);

            // Create the upload request.
            using var request = new HttpRequestMessage(
                HttpMethod.Post, "api/Profile/picture");

            request.Content = content;

            // Forward the user's API session cookie.
            AddSessionCookie(request);

            // Send the image to the API.
            using var response = await _httpClient.SendAsync(request);

            // Return false if the upload failed.
            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return false;
            }

            return true;
        }

        // Retrieves the current user's profile picture from the API.
        public async Task<(byte[] Content, string ContentType)?> GetProfilePictureAsync()
        {
            LastError = null;

            // Create the image retrieval request.
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "api/Profile/picture");

            // Forward the user's API session cookie.
            AddSessionCookie(request);

            // Send the request to the API.
            using var response = await _httpClient.SendAsync(request);

            // Return null if the image cannot be retrieved.
            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return null;
            }

            // Read the image content.
            byte[] imageBytes =
                await response.Content.ReadAsByteArrayAsync();

            // Get the image content type.
            string contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            return (imageBytes, contentType);
        }

        // Adds the logged-in user's API session cookie to a request.
        private void AddSessionCookie(HttpRequestMessage request)
        {
            // Access the current MVC session.
            var session = _httpContextAccessor.HttpContext?.Session;

            // Retrieve the saved API session cookie.
            string? apiCookie =
                session?.GetString("ApiSessionCookie");

            // Forward the cookie when it exists.
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }
        }

        // Updates the logged-in user's profile through the API.
        public async Task<bool> UpdateProfileAsync(
            string fullName,
            string email,
            string? phoneNumber)
        {
            LastError = null;

            // Prepare the updated profile information.
            var profileData = new
            {
                fullName,
                email,
                phoneNumber
            };

            // Create an authenticated PUT request.
            using var request = new HttpRequestMessage(
                HttpMethod.Put, "api/Profile");

            request.Content = JsonContent.Create(profileData);

            // Forward the logged-in user's API session cookie.
            AddSessionCookie(request);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastError = await response.Content.ReadAsStringAsync();
                return false;
            }

            return true;
        }
    }
}