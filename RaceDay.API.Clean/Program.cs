using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using Microsoft.AspNetCore.Identity;// Provides ASP.NET Core password hashing functionality.
using RaceDay.API.Clean.Models;// Provides access to the RaceDay User model.
using RaceDay.API.Clean.Services;// Provides access to the authentication service.

namespace RaceDay.API.Clean
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Sets the environment to Development so Swagger can be enabled.
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");


        // Creates the application builder.
        var builder = WebApplication.CreateBuilder(args);

            // Registers the RaceDay database connection with Entity Framework Core.
            builder.Services.AddDbContext<RaceDayDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("RaceDayConnection")));

            // Registers API controllers before the application is built.
            builder.Services.AddControllers();
            
            // Registers the password hasher used to secure user passwords.
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Registers the authentication service for dependency injection.
            builder.Services.AddScoped<AuthService>();

            // Registers the services required by Swagger.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Builds the application after all services have been registered.
            var app = builder.Build();

            // Displays the current environment and configured application URLs.
            Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
            Console.WriteLine($"Application URLs: {string.Join(", ", app.Urls)}");

            // Enables Swagger in the Development environment.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // HTTPS redirection remains disabled while we test HTTP locally.
            // app.UseHttpsRedirection();

            // Enables authorization middleware.
            app.UseAuthorization();

            // Maps controller endpoints, including DatabaseTestController.
            app.MapControllers();

            // Starts the application.
            app.Run();
        }
    }


}
