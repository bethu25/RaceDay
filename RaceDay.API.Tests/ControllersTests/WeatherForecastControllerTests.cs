using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using RaceDay.API.Clean.Controllers;
using Xunit;
using System.Linq;

namespace RaceDay.API.Tests
{
    public class WeatherForecastControllerTests
    {
        [Fact]
        public void Get_ReturnsFiveWeatherForecasts()
        {
            // Arrange: Create the controller using a built-in logger.
            var logger = NullLogger<WeatherForecastController>.Instance;
            var controller = new WeatherForecastController(logger);

            // Act: Call the Get method and store the forecasts.
            var forecasts = controller.Get().ToList();

            // Assert: Check that exactly five forecasts are returned.
            Assert.Equal(5, forecasts.Count);
        }

        [Fact]
        public void Get_ReturnsForecastsWithRequiredProperties()
        {
            // Arrange: Create the controller using a built-in logger.
            var logger = NullLogger<WeatherForecastController>.Instance;
            var controller = new WeatherForecastController(logger);

            // Act: Retrieve the weather forecasts.
            var forecasts = controller.Get().ToList();

            // Assert: Check that each forecast has valid property values.
            Assert.All(forecasts, forecast =>
            {
                Assert.NotEqual(default, forecast.Date);
                Assert.InRange(forecast.TemperatureC, -20, 54);
                Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
            });
        }
    }
}
