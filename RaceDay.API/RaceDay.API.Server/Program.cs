var builder = WebApplication.CreateBuilder(args);

// Add Swagger services.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger.
app.UseSwagger();
app.UseSwaggerUI();

// Test endpoint.
app.MapGet("/api/test", () =>
{
    return "RaceDay API is running successfully!";
})
.WithName("TestRaceDayAPI")
.WithOpenApi();

app.Run();