using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.Models;
using RaceDay.API.Clean.Services;
using Azure.Storage.Blobs;
using RaceDay.API.Clean.Services;

namespace RaceDay.API.Clean
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Read the Azure Storage connection string from User Secrets.
            string? azureConnectionString =
                builder.Configuration["AzureStorage:ConnectionString"];

            // Stop the application if the Azure connection string is missing.
            if (string.IsNullOrWhiteSpace(azureConnectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            // Register the Azure Blob Storage client.
            builder.Services.AddSingleton(
                new BlobServiceClient(azureConnectionString));

            // Register the service that handles file uploads.
            builder.Services.AddScoped<BlobStorageService>();

            // Register controllers for handling API requests.
            builder.Services.AddControllers();

            // Register Swagger for testing API endpoints.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Connect the API to the RaceDay SQL Server database.
            builder.Services.AddDbContext<RaceDayDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("RaceDayConnection")));

            // Register password hashing for securely storing user passwords.
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Register the authentication service.
            builder.Services.AddScoped<AuthService>();

            // Enable in-memory storage for user sessions.
            builder.Services.AddDistributedMemoryCache();

            // Configure session settings.
            builder.Services.AddSession(options =>
            {
                // Automatically expire inactive sessions after 30 minutes.
                options.IdleTimeout = TimeSpan.FromMinutes(30);

                // Prevent JavaScript from accessing the session cookie.
                options.Cookie.HttpOnly = true;

                // Use HTTPS security when the request uses HTTPS.
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                // Allow the cookie to be sent with normal same-site requests.
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            var app = builder.Build();

            // Enable Swagger when running in Development mode.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable session management before processing API requests.
            app.UseSession();

            // Enable authorization for protected API endpoints.
            app.UseAuthorization();

            // Map API controllers to their routes.
            app.MapControllers();

            // Start the API application.
            app.Run();
        }
    }
}