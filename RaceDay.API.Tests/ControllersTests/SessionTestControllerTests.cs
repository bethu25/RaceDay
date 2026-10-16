using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Clean.Controllers;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class SessionTestControllerTests
    {
        // Creates the controller with a simulated HTTP session.
        private static SessionTestController CreateController(
            int? userId = null, string? userRole = null)
        {
            var controller = new SessionTestController();

            var httpContext = new DefaultHttpContext();

            // Attach a test session to the HTTP context.
            var session = new TestSession();

            // Only store the user ID when one is supplied.
            if (userId.HasValue)
            {
                session.Set("UserID", BitConverter.GetBytes(userId.Value));
            }

            // Store the role when one is supplied.
            if (userRole != null)
            {
                session.Set(
                    "UserRole",
                    System.Text.Encoding.UTF8.GetBytes(userRole));
            }

            httpContext.Session = session;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // Simple session implementation used by these tests.
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionData = new();

            public bool IsAvailable => true;

            public string Id => "TestSession";

            public IEnumerable<string> Keys => _sessionData.Keys;

            public void Clear()
            {
                _sessionData.Clear();
            }

            public Task CommitAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task LoadAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Remove(string key)
            {
                _sessionData.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _sessionData[key] = value;
            }

            public bool TryGetValue(string key, out byte[]? value)
            {
                return _sessionData.TryGetValue(key, out value);
            }
        }

        // TEST 1: An active session should return OK.
        [Fact]
        public void GetSession_ActiveSession_ReturnsOk()
        {
            var controller = CreateController(1, "Participant");

            var response = controller.GetSession();

            var okResult = Assert.IsType<OkObjectResult>(response);

            Assert.NotNull(okResult.Value);
        }

        // TEST 2: A session without a user ID should return Unauthorized.
        [Fact]
        public void GetSession_NoUserId_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var response = controller.GetSession();

            Assert.IsType<UnauthorizedObjectResult>(response);
        }
    }
}
