using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.API.Tests
{
    public class ResultsControllerTests
    {
        // Creates a separate in-memory database for each test.
        private static RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // Creates a controller with a logged-in user's session.
        private static ResultsController CreateController(
            RaceDayDbContext context, int userId, string role)
        {
            var controller = new ResultsController(context);

            // Give the controller an HTTP context so session values can be read.
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            // Store the logged-in user's ID and role in the session.
            httpContext.Session.SetInt32("UserID", userId);
            httpContext.Session.SetString("UserRole", role);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // Adds a basic event, category and enrolment for result-related tests.
        private static async Task<int> AddEnrolmentAsync(
            RaceDayDbContext context, int organiserId, int participantId)
        {
            var raceEvent = new RaceDay.API.Clean.Models.Event
            {
                EventName = "Test Race",
                EventDate = DateTime.Today.AddDays(7),
                Location = "Johannesburg",
                OrganiserID = organiserId
            };

            context.Events.Add(raceEvent);
            await context.SaveChangesAsync();

            var category = new Category
            {
                CategoryName = "10km",
                EventID = raceEvent.EventID
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = category.CategoryID
            };

            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            return enrolment.EnrolmentID;
        }

        [Fact]
        public async Task GetResults_Participant_ReturnsOwnResults()
        {
            // Arrange: Create an enrolment and a result for the participant.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            context.Results.Add(new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(60),
                Position = 1
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 10, "Participant");

            // Act: Retrieve the participant's results.
            var response = await controller.GetResults();

            // Assert: The response should be successful and contain one result.
            var okResult = Assert.IsType<OkObjectResult>(response);
            var results = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Single(results);
        }

        [Fact]
        public async Task GetResults_Organiser_ReturnsResultsForOwnEvents()
        {
            // Arrange: Add a result to an event owned by organiser 1.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            context.Results.Add(new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(55),
                Position = 1
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 1, "Organiser");

            // Act: Retrieve results belonging to this organiser's events.
            var response = await controller.GetResults();

            // Assert: The organiser should receive the result.
            var okResult = Assert.IsType<OkObjectResult>(response);
            var results = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);

            Assert.Single(results);
        }

        [Fact]
        public async Task GetResults_UnknownRole_ReturnsForbid()
        {
            // Arrange: Use a role that the controller does not support.
            using var context = CreateDatabase();
            var controller = CreateController(context, 1, "Unknown");

            // Act: Request the results.
            var response = await controller.GetResults();

            // Assert: The controller should deny access.
            Assert.IsType<ForbidResult>(response);
        }

        [Fact]
        public async Task CreateResult_ValidEnrolment_CreatesResult()
        {
            // Arrange: Create an enrolment owned by the organiser's event.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            var controller = CreateController(context, 1, "Organiser");

            var request = new ResultRequest
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(50),
                Position = 1
            };

            // Act: Create the result.
            var response = await controller.CreateResult(request);

            // Assert: The controller should return Created and save the result.
            Assert.IsType<CreatedResult>(response);
            Assert.Equal(1, await context.Results.CountAsync());
        }

        [Fact]
        public async Task CreateResult_EnrolmentNotOwned_ReturnsNotFound()
        {
            // Arrange: Create an enrolment owned by organiser 1.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            // Organiser 2 does not own the event.
            var controller = CreateController(context, 2, "Organiser");

            var request = new ResultRequest
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(50),
                Position = 1
            };

            // Act: Attempt to create a result for an event owned by someone else.
            var response = await controller.CreateResult(request);

            // Assert: The controller should reject the request.
            Assert.IsType<NotFoundObjectResult>(response);
        }

        [Fact]
        public async Task CreateResult_DuplicateResult_ReturnsConflict()
        {
            // Arrange: Create an enrolment that already has a result.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            context.Results.Add(new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(50),
                Position = 1
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context, 1, "Organiser");

            var request = new ResultRequest
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(48),
                Position = 1
            };

            // Act: Try to add a second result for the same enrolment.
            var response = await controller.CreateResult(request);

            // Assert: The controller should prevent duplicate results.
            Assert.IsType<ConflictObjectResult>(response);
        }

        [Fact]
        public async Task UpdateResult_ValidRequest_UpdatesResult()
        {
            // Arrange: Create an enrolment and its existing result.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(60),
                Position = 2
            };

            context.Results.Add(result);
            await context.SaveChangesAsync();

            var controller = CreateController(context, 1, "Organiser");

            var request = new ResultRequest
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(55),
                Position = 1
            };

            // Act: Update the existing result.
            var response = await controller.UpdateResult(result.ResultID, request);

            // Assert: The update should succeed and save the new values.
            Assert.IsType<OkObjectResult>(response);

            var updatedResult = await context.Results.FindAsync(result.ResultID);

            Assert.NotNull(updatedResult);
            Assert.Equal(TimeSpan.FromMinutes(55), updatedResult.FinishTime);
            Assert.Equal(1, updatedResult.Position);
        }

        [Fact]
        public async Task UpdateResult_ResultNotOwned_ReturnsNotFound()
        {
            // Arrange: Create a result owned by organiser 1.
            using var context = CreateDatabase();
            int enrolmentId = await AddEnrolmentAsync(context, 1, 10);

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(60),
                Position = 2
            };

            context.Results.Add(result);
            await context.SaveChangesAsync();

            // Organiser 2 cannot update organiser 1's result.
            var controller = CreateController(context, 2, "Organiser");

            var request = new ResultRequest
            {
                EnrolmentID = enrolmentId,
                FinishTime = TimeSpan.FromMinutes(55),
                Position = 1
            };

            // Act: Attempt to update a result the organiser does not own.
            var response = await controller.UpdateResult(result.ResultID, request);

            // Assert: The controller should return Not Found.
            Assert.IsType<NotFoundObjectResult>(response);
        }

        // A small session implementation used by the controller tests.
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionValues = new();

            public IEnumerable<string> Keys => _sessionValues.Keys;

            public string Id => Guid.NewGuid().ToString();

            public bool IsAvailable => true;

            public void Clear() => _sessionValues.Clear();

            public Task CommitAsync(
                System.Threading.CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task LoadAsync(
                System.Threading.CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public void Remove(string key) => _sessionValues.Remove(key);

            public void Set(string key, byte[] value)
                => _sessionValues[key] = value;

            public bool TryGetValue(string key, out byte[]? value)
                => _sessionValues.TryGetValue(key, out value);
        }
    }
}

