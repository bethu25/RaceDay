using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class RoutesControllerTests
    {
        // Creates a separate temporary database for each test.
        private RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
        }

        // Creates the controller and simulates an Organiser's session.
        private RoutesController CreateController(
            RaceDayDbContext context, int? organiserId = null)
        {
            var controller = new RoutesController(context);

            // Create an HTTP context for the simulated request.
            var httpContext = new DefaultHttpContext();

            // Attach a test session to the HTTP context.
            httpContext.Session = new TestSession();

            // Store the Organiser's ID in the session when provided.
            if (organiserId.HasValue)
            {
                httpContext.Session.SetInt32("UserID", organiserId.Value);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // Adds a sample event owned by Organiser 1.
        private async Task AddSampleEvent(RaceDayDbContext context)
        {
            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "RaceDay Test Event",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Pretoria",
                Description = "Test event for route operations",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            await context.SaveChangesAsync();
        }

        // TEST 1: Check that all routes can be retrieved.
        [Fact]
        public async Task GetRoutes_ReturnsAllRoutes()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add an event for the sample routes.
            await AddSampleEvent(context);

            // Add two routes to the database.
            context.Routes.AddRange(
                new RaceDay.API.Clean.Models.Route
                {
                    RouteID = 1,
                    EventID = 1,
                    RouteName = "Pretoria 5 km Route",
                    Distance = 5,
                    RouteDescription = "Short running route"
                },
                new RaceDay.API.Clean.Models.Route
                {
                    RouteID = 2,
                    EventID = 2,
                    RouteName = "Pretoria 10 km Route",
                    Distance = 10,
                    RouteDescription = "Long running route"
                });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act: Retrieve all routes.
            var result = await controller.GetRoutes();

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Both routes should be returned.
            var routes = Assert.IsAssignableFrom<
                IEnumerable<RaceDay.API.Clean.Models.Route>>(
                okResult.Value);

            Assert.Equal(2, routes.Count());
        }

        // TEST 2: Check that an existing route can be retrieved.
        [Fact]
        public async Task GetRoute_ExistingRoute_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEvent(context);

            context.Routes.Add(new RaceDay.API.Clean.Models.Route
            {
                RouteID = 1,
                EventID = 1,
                RouteName = "Pretoria 10 km Route",
                Distance = 10,
                RouteDescription = "A running route in Pretoria"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act: Retrieve route 1.
            var result = await controller.GetRoute(1);

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            var route = Assert.IsType<
                RaceDay.API.Clean.Models.Route>(okResult.Value);

            Assert.Equal(1, route.RouteID);
            Assert.Equal("Pretoria 10 km Route", route.RouteName);
        }

        // TEST 3: Check that a missing route returns HTTP 404.
        [Fact]
        public async Task GetRoute_NonExistingRoute_ReturnsNotFound()
        {
            // Arrange: Create an empty database.
            using var context = CreateDatabase();

            var controller = CreateController(context);

            // Act: Request a route that does not exist.
            var result = await controller.GetRoute(999);

            // Assert: The response should be HTTP 404 Not Found.
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // TEST 4: Check that an Organiser can create a route
        // for an event they own.
        [Fact]
        public async Task CreateRoute_ValidOrganiser_ReturnsCreatedResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEvent(context);

            var controller = CreateController(context, 1);

            // Prepare the route details.
            var request = new RouteRequest
            {
                EventID = 1,
                RouteName = "Pretoria 10 km Route",
                Distance = 10,
                RouteDescription = "A running route in Pretoria"
            };

            // Act: Create the route.
            var result = await controller.CreateRoute(request);

            // Assert: The response should be HTTP 201 Created.
            Assert.IsType<CreatedAtActionResult>(result);

            // Assert: The route should be saved.
            Assert.Equal(1, await context.Routes.CountAsync());

            var savedRoute = await context.Routes.FirstAsync();

            Assert.Equal("Pretoria 10 km Route", savedRoute.RouteName);
            Assert.Equal(10, savedRoute.Distance);
            Assert.Equal(1, savedRoute.EventID);
        }

        // TEST 5: Check that an event cannot have duplicate routes.
        [Fact]
        public async Task CreateRoute_EventAlreadyHasRoute_ReturnsConflict()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEvent(context);

            // Add a route that already belongs to the event.
            context.Routes.Add(new RaceDay.API.Clean.Models.Route
            {
                RouteID = 1,
                EventID = 1,
                RouteName = "Existing Route",
                Distance = 10,
                RouteDescription = "Existing route description"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 1);

            var request = new RouteRequest
            {
                EventID = 1,
                RouteName = "Duplicate Route",
                Distance = 10,
                RouteDescription = "Duplicate route description"
            };

            // Act: Attempt to create a second route for the same event.
            var result = await controller.CreateRoute(request);

            // Assert: The controller should return HTTP 409 Conflict.
            Assert.IsType<ConflictObjectResult>(result);

            // Assert: The existing route should remain the only route.
            Assert.Equal(1, await context.Routes.CountAsync());
        }

        // TEST 6: Check that an Organiser can update their route.
        [Fact]
        public async Task UpdateRoute_ValidOrganiser_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEvent(context);

            context.Routes.Add(new RaceDay.API.Clean.Models.Route
            {
                RouteID = 1,
                EventID = 1,
                RouteName = "Old Route Name",
                Distance = 5,
                RouteDescription = "Old route description"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 1);

            // Prepare the updated route details.
            var request = new RouteRequest
            {
                EventID = 1,
                RouteName = "Updated Pretoria Route",
                Distance = 10,
                RouteDescription = "Updated route description"
            };

            // Act: Update route 1.
            var result = await controller.UpdateRoute(1, request);

            // Assert: The response should be HTTP 200 OK.
            Assert.IsType<OkObjectResult>(result);

            // Assert: The database should contain the updated information.
            var updatedRoute = await context.Routes.FirstAsync();

            Assert.Equal("Updated Pretoria Route", updatedRoute.RouteName);
            Assert.Equal(10, updatedRoute.Distance);
            Assert.Equal("Updated route description",
                updatedRoute.RouteDescription);
        }

        // TEST 7: Check that an Organiser can delete their route.
        [Fact]
        public async Task DeleteRoute_ExistingOwnedRoute_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEvent(context);

            context.Routes.Add(new RaceDay.API.Clean.Models.Route
            {
                RouteID = 1,
                EventID = 1,
                RouteName = "Route to Delete",
                Distance = 10,
                RouteDescription = "Temporary route"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 1);

            // Act: Delete the route.
            var result = await controller.DeleteRoute(1);

            // Assert: The response should be HTTP 200 OK.
            Assert.IsType<OkObjectResult>(result);

            // Assert: The route should be removed from the database.
            Assert.Equal(0, await context.Routes.CountAsync());
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
    }

}
