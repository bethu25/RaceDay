using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.Models;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class DatabaseTestControllerTests
    {
        // Creates a separate in-memory database for each test.
        private static RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // TEST 1: The controller should return all users.
        [Fact]
        public async Task GetUsers_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            // Add a sample user to the test database.
            context.Users.Add(new User
            {
                UserID = 1,
                FullName = "Test Runner",
                Email = "runner@example.com",
                Password = "hashed-password",
                Role = "Participant",
                PhoneNumber = "0821234567"
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(response);

            // Check that the returned collection contains one user.
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(
                okResult.Value);

            Assert.Single(users);
        }

        // TEST 2: The controller should return all events.
        [Fact]
        public async Task GetEvents_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            context.Events.Add(new RaceDay.API.Clean.Models.Event
            {
                EventID = 1,
                EventName = "City Run",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Pretoria",
                Description = "A local running event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetEvents();

            var okResult = Assert.IsType<OkObjectResult>(response);

            var events =
                Assert.IsAssignableFrom<IEnumerable<RaceDay.API.Clean.Models.Event>>(
                    okResult.Value);

            Assert.Single(events);
        }

        // TEST 3: The controller should return all categories.
        [Fact]
        public async Task GetCategories_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            // Add the event required by the sample category.
            context.Events.Add(new RaceDay.API.Clean.Models.Event
            {
                EventID = 1,
                EventName = "City Run",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Pretoria",
                Description = "A local running event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km",
                Distance = 10,
                EntryFee = 100,
                MaxParticipants = 100
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetCategories();

            var okResult = Assert.IsType<OkObjectResult>(response);

            var categories =
                Assert.IsAssignableFrom<IEnumerable<Category>>(
                    okResult.Value);

            Assert.Single(categories);
        }

        // TEST 4: The controller should return all enrolments.
        [Fact]
        public async Task GetEnrolments_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            // Add the event and category required by the enrolment.
            context.Events.Add(new RaceDay.API.Clean.Models.Event
            {
                EventID = 1,
                EventName = "City Run",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Pretoria",
                Description = "A local running event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km",
                Distance = 10,
                EntryFee = 100,
                MaxParticipants = 100
            });

            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 2,
                CategoryID = 1,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetEnrolments();

            var okResult = Assert.IsType<OkObjectResult>(response);

            var enrolments =
                Assert.IsAssignableFrom<IEnumerable<Enrolment>>(
                    okResult.Value);

            Assert.Single(enrolments);
        }

        // TEST 5: The controller should return all results.
        [Fact]
        public async Task GetResults_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            // Add the related event, category and enrolment.
            context.Events.Add(new RaceDay.API.Clean.Models.Event
            {
                EventID = 1,
                EventName = "City Run",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Pretoria",
                Description = "A local running event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km",
                Distance = 10,
                EntryFee = 100,
                MaxParticipants = 100
            });

            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 2,
                CategoryID = 1,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            });

            context.Results.Add(new Result
            {
                ResultID = 1,
                EnrolmentID = 1,
                FinishTime = TimeSpan.FromMinutes(60),
                Position = 1
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetResults();

            var okResult = Assert.IsType<OkObjectResult>(response);

            var results =
                Assert.IsAssignableFrom<IEnumerable<Result>>(
                    okResult.Value);

            Assert.Single(results);
        }

        // TEST 6: The controller should return all routes.
        [Fact]
        public async Task GetRoutes_ReturnsOkResult()
        {
            using var context = CreateDatabase();

            // Add the event required by the sample route.
            context.Events.Add(new RaceDay.API.Clean.Models.Event
            {
                EventID = 1,
                EventName = "City Run",
                EventDate = new DateTime(2026, 11, 15),
                Location = "Pretoria",
                Description = "A local running event",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            context.Routes.Add(new RaceDay.API.Clean.Models.Route
            {
                RouteID = 1,
                EventID = 1,
                RouteName = "City Route",
                Distance = 10,
                RouteDescription = "A route through the city"
            });

            await context.SaveChangesAsync();

            var controller = new DatabaseTestController(context);

            var response = await controller.GetRoutes();

            var okResult = Assert.IsType<OkObjectResult>(response);

            var routes =
                Assert.IsAssignableFrom<IEnumerable<RaceDay.API.Clean.Models.Route>>(
                    okResult.Value);

            Assert.Single(routes);
        }
    }
}
