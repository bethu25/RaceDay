using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class EventsControllerTests
    {
        // Creates a separate temporary database for each test.
        private RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // TEST 1: Check that an existing event can be retrieved.
        [Fact]
        public async Task GetEvent_ExistingEvent_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Add a sample event.
            var sampleEvent = new Event
            {
                EventID = 1,
                EventName = "RaceDay Test Run",
                Location = "Pretoria",
                Description = "Unit testing event",
                Distance = 5,
                EventType = "Running",
                EventDate = DateTime.Now,
                OrganiserID = 1
            };

            context.Events.Add(sampleEvent);
            await context.SaveChangesAsync();

            // Arrange: Create the controller.
            var controller = new EventsController(context);

            // Provide an HTTP context for the controller.
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act: Request the existing event.
            var result = await controller.GetEvent(1);

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: The returned event should have the correct details.
            var returnedEvent = Assert.IsType<Event>(okResult.Value);

            Assert.Equal(1, returnedEvent.EventID);
            Assert.Equal("RaceDay Test Run", returnedEvent.EventName);
        }

        // TEST 2: Check that a missing event returns HTTP 404.
        [Fact]
        public async Task GetEvent_NonExistingEvent_ReturnsNotFound()
        {
            // Arrange: Create an empty temporary database.
            using var context = CreateDatabase();

            // Arrange: Create the controller.
            var controller = new EventsController(context);

            // Provide an HTTP context for the controller.
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act: Request an event that does not exist.
            var result = await controller.GetEvent(999);

            // Assert: The response should be HTTP 404 Not Found.
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // TEST 3: Check that a valid Organiser can create an event.
        [Fact]
        public async Task CreateEvent_ValidOrganiser_ReturnsCreatedResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Add a valid Organiser to the database.
            var organiser = new User
            {
                UserID = 1,
                FullName = "Test Organiser",
                Email = "organiser@raceday.test",
                Password = "TestPassword123",
                Role = "Organiser",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(organiser);
            await context.SaveChangesAsync();

            // Arrange: Create the controller.
            var controller = new EventsController(context);

            // Create an HTTP context for the simulated session.
            var httpContext = new DefaultHttpContext();

            // Create a session containing the Organiser's ID.
            var session = new TestSession();
            session.SetInt32("UserID", organiser.UserID);

            // Attach the test session to the HTTP context.
            httpContext.Features.Set<ISessionFeature>(
                new TestSessionFeature { Session = session });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Arrange: Prepare the event information.
            var request = new EventRequest
            {
                EventName = "RaceDay Test Run",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Pretoria",
                Description = "Unit testing event",
                Distance = 5,
                EventType = "Running"
            };

            // Act: Create the event.
            var result = await controller.CreateEvent(request);

            // Assert: The controller should return HTTP 201 Created.
            Assert.IsType<CreatedAtActionResult>(result);

            // Assert: Exactly one event should be saved.
            Assert.Equal(1, await context.Events.CountAsync());

            // Assert: The saved event should have the correct information.
            var savedEvent = await context.Events.FirstAsync();

            Assert.Equal("RaceDay Test Run", savedEvent.EventName);
            Assert.Equal("Pretoria", savedEvent.Location);
            Assert.Equal(organiser.UserID, savedEvent.OrganiserID);
        }

        // Simulates an HTTP session for unit testing.
        private class TestSession : ISession
        {
            // Stores session values in memory.
            private readonly Dictionary<string, byte[]> _values = new();

            public bool IsAvailable => true;

            public string Id => "TestSession";

            public IEnumerable<string> Keys => _values.Keys;

            public void Clear()
            {
                _values.Clear();
            }

            public Task LoadAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task CommitAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void Remove(string key)
            {
                _values.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _values[key] = value;
            }

            public bool TryGetValue(string key, out byte[]? value)
            {
                return _values.TryGetValue(key, out value);
            }
        }

        // Makes the simulated session available to the HTTP context.
        private class TestSessionFeature : ISessionFeature
        {
            public ISession Session { get; set; } = null!;
        }
    }
}

