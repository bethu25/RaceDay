using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Clean.Controllers;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class AccessControlTestControllerTests
    {
        // TEST 1: The organiser endpoint should return an OK response.
        [Fact]
        public void OrganiserAccess_ReturnsOk()
        {
            // Arrange: Create the controller.
            var controller = new AccessControlTestController();

            // Act: Call the organiser endpoint.
            var response = controller.OrganiserAccess();

            // Assert: Confirm that the response is HTTP 200 OK.
            Assert.IsType<OkObjectResult>(response);
        }

        // TEST 2: The participant endpoint should return an OK response.
        [Fact]
        public void ParticipantAccess_ReturnsOk()
        {
            // Arrange: Create the controller.
            var controller = new AccessControlTestController();

            // Act: Call the participant endpoint.
            var response = controller.ParticipantAccess();

            // Assert: Confirm that the response is HTTP 200 OK.
            Assert.IsType<OkObjectResult>(response);
        }
    }
}