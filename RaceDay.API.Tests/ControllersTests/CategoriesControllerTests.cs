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
    public class CategoriesControllerTests
    {
        // Creates a separate temporary database for each test.
        private RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
        }

        // Creates a controller with a simulated HTTP session.
        private CategoriesController CreateController(
            RaceDayDbContext context, int? userId = null)
        {
            // Create the controller.
            var controller = new CategoriesController(context);

            // Create a simulated HTTP context.
            var httpContext = new DefaultHttpContext();

            // Enable session access for controller methods.
            httpContext.Session = new TestSession();

            // Store the Organiser's ID when one is provided.
            if (userId.HasValue)
            {
                httpContext.Session.SetInt32("UserID", userId.Value);
            }

            // Attach the HTTP context to the controller.
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // TEST 1: Check that all categories can be retrieved.
        [Fact]
        public async Task GetCategories_ReturnsAllCategories()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add sample categories.
            context.Categories.AddRange(
                new Category
                {
                    CategoryID = 1,
                    EventID = 1,
                    CategoryName = "5 km Run",
                    Distance = 5,
                    EntryFee = 100,
                    MaxParticipants = 100
                },
                new Category
                {
                    CategoryID = 2,
                    EventID = 1,
                    CategoryName = "10 km Run",
                    Distance = 10,
                    EntryFee = 150,
                    MaxParticipants = 200
                });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act: Retrieve all categories.
            var result = await controller.GetCategories();

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Both categories should be returned.
            var categories = Assert.IsAssignableFrom<
                IEnumerable<Category>>(okResult.Value);

            Assert.Equal(2, categories.Count());
        }

        // TEST 2: Check that an existing category can be retrieved.
        [Fact]
        public async Task GetCategory_ExistingCategory_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add a sample category.
            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "5 km Run",
                Distance = 5,
                EntryFee = 100,
                MaxParticipants = 100
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act: Retrieve category 1.
            var result = await controller.GetCategory(1);

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: The correct category should be returned.
            var category = Assert.IsType<Category>(okResult.Value);

            Assert.Equal(1, category.CategoryID);
            Assert.Equal("5 km Run", category.CategoryName);
        }

        // TEST 3: Check that a missing category returns HTTP 404.
        [Fact]
        public async Task GetCategory_NonExistingCategory_ReturnsNotFound()
        {
            // Arrange: Create an empty database.
            using var context = CreateDatabase();

            var controller = CreateController(context);

            // Act: Request a category that does not exist.
            var result = await controller.GetCategory(999);

            // Assert: The response should be HTTP 404 Not Found.
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // TEST 4: Check that an Organiser can create a category
        // for an event that belongs to them.
        [Fact]
        public async Task CreateCategory_ValidOrganiser_ReturnsCreatedResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add an event owned by Organiser 1.
            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "RaceDay Test Event",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Pretoria",
                Description = "Testing event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            await context.SaveChangesAsync();

            // Simulate Organiser 1 being logged in.
            var controller = CreateController(context, 1);

            // Prepare the category details.
            var request = new CategoryRequest
            {
                EventID = 1,
                CategoryName = "10 km Run",
                Distance = 10,
                EntryFee = 150,
                MaxParticipants = 200
            };

            // Act: Create the category.
            var result = await controller.CreateCategory(request);

            // Assert: The response should be HTTP 201 Created.
            Assert.IsType<CreatedAtActionResult>(result);

            // Assert: The category should be saved.
            Assert.Equal(1, await context.Categories.CountAsync());

            var savedCategory = await context.Categories.FirstAsync();

            Assert.Equal("10 km Run", savedCategory.CategoryName);
            Assert.Equal(150, savedCategory.EntryFee);
            Assert.Equal(1, savedCategory.EventID);
        }

        // TEST 5: Check that an Organiser cannot create a category
        // for an event they do not own.
        [Fact]
        public async Task CreateCategory_EventNotOwnedByOrganiser_ReturnsNotFound()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add an event owned by Organiser 2.
            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "Another Organiser's Event",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Johannesburg",
                Description = "Testing event ownership",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 2
            });

            await context.SaveChangesAsync();

            // Simulate Organiser 1 being logged in.
            var controller = CreateController(context, 1);

            var request = new CategoryRequest
            {
                EventID = 1,
                CategoryName = "10 km Run",
                Distance = 10,
                EntryFee = 150,
                MaxParticipants = 200
            };

            // Act: Attempt to create a category for another Organiser's event.
            var result = await controller.CreateCategory(request);

            // Assert: The request should be rejected.
            Assert.IsType<NotFoundObjectResult>(result);

            // Assert: No category should have been saved.
            Assert.Equal(0, await context.Categories.CountAsync());
        }

        // TEST 6: Check that an Organiser can delete their category.
        [Fact]
        public async Task DeleteCategory_ExistingOwnedCategory_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Add an event and its category.
            var eventItem = new Event
            {
                EventID = 1,
                EventName = "RaceDay Test Event",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Pretoria",
                Description = "Testing event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            };

            context.Events.Add(eventItem);

            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km Run",
                Distance = 10,
                EntryFee = 150,
                MaxParticipants = 200
            });

            await context.SaveChangesAsync();

            // Simulate the event's Organiser being logged in.
            var controller = CreateController(context, 1);

            // Act: Delete the category.
            var result = await controller.DeleteCategory(1);

            // Assert: The response should be HTTP 200 OK.
            Assert.IsType<OkObjectResult>(result);

            // Assert: The category should no longer exist.
            Assert.Equal(0, await context.Categories.CountAsync());
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
