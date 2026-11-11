using RaceDay.MVC.Services;

namespace RaceDay.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register MVC controllers and views.
            builder.Services.AddControllersWithViews();

            // Allow services to access the current user's MVC session.
            builder.Services.AddHttpContextAccessor();

            // Enable in-memory storage for MVC sessions.
            builder.Services.AddDistributedMemoryCache();

            // Configure MVC session settings.
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Register the HTTP client used to communicate with the API.
            builder.Services.AddHttpClient("RaceDayApi", client =>
            {
                string? apiBaseUrl =
                    builder.Configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrWhiteSpace(apiBaseUrl))
                {
                    throw new InvalidOperationException(
                        "The RaceDay API base URL is missing.");
                }

                client.BaseAddress = new Uri(apiBaseUrl);
            })
            // Disable automatic cookie handling because each MVC user
            // must forward their own API session cookie.
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    UseCookies = false
                });

            // Register the services used by MVC controllers.
            builder.Services.AddScoped<EventApiService>();
            builder.Services.AddScoped<AuthApiService>();
            builder.Services.AddScoped<EnrolmentApiService>();
            builder.Services.AddScoped<ResultApiService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable sessions before routing and controller execution.
            app.UseSession();

            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}