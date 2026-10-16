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
    public class EnrolmentsControllerTests
    {
        // Creates a separate temporary database for each test.
        private RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
        }

        // Creates the controller with a simulated logged-in user.
        private EnrolmentsController CreateController(
            RaceDayDbContext context,
            int userId,
            string role)
        {
            var controller = new EnrolmentsController(context);

            // Create an HTTP context for the simulated request.
            var httpContext = new DefaultHttpContext();

            // Create a test session and store the user's details.
            var session = new TestSession();
            session.SetInt32("UserID", userId);
            session.SetString("UserRole", role);

            // Attach the session to the HTTP context.
            httpContext.Session = session;

            // Attach the HTTP context to the controller.
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // Adds an event owned by Organiser 1 and a category for that event.
        private async Task AddSampleEventAndCategory(
            RaceDayDbContext context,
            int maxParticipants = 10)
        {
            // Add a sample event.
            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "RaceDay Test Event",
                EventDate = DateTime.Now.AddDays(7),
                Location = "Pretoria",
                Description = "Test event for enrolments",
                Distance = 10,
                EventType = "Running",
                OrganiserID = 1
            });

            // Add a category for the event.
            context.Categories.Add(new Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10 km Run",
                Distance = 10,
                EntryFee = 150,
                MaxParticipants = maxParticipants
            });

            await context.SaveChangesAsync();
        }

        // TEST 1: Participants should see only their own enrolments.
        [Fact]
        public async Task GetEnrolments_Participant_ReturnsOwnEnrolments()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            // Add enrolments belonging to two different participants.
            context.Enrolments.AddRange(
                new Enrolment
                {
                    EnrolmentID = 1,
                    ParticipantID = 10,
                    CategoryID = 1,
                    EnrolmentDate = DateTime.Today,
                    Status = "Active"
                },
                new Enrolment
                {
                    EnrolmentID = 2,
                    ParticipantID = 20,
                    CategoryID = 1,
                    EnrolmentDate = DateTime.Today,
                    Status = "Active"
                });

            await context.SaveChangesAsync();

            // Simulate Participant 10 being logged in.
            var controller = CreateController(
                context, 10, "Participant");

            // Act: Retrieve the participant's enrolments.
            var result = await controller.GetEnrolments();

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: Only one enrolment should be returned.
            var enrolments = Assert.IsAssignableFrom<
                System.Collections.IEnumerable>(okResult.Value);

            Assert.Single(enrolments.Cast<object>());
        }

        // TEST 2: Organisers should see enrolments for their own events.
        [Fact]
        public async Task GetEnrolments_Organiser_ReturnsOwnEventEnrolments()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            // Add a participant's enrolment in the Organiser's event.
            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 10,
                CategoryID = 1,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            // Simulate Organiser 1 being logged in.
            var controller = CreateController(
                context, 1, "Organiser");

            // Act: Retrieve the enrolments for the Organiser's events.
            var result = await controller.GetEnrolments();

            // Assert: The response should be HTTP 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert: The enrolment should be returned.
            var enrolments = Assert.IsAssignableFrom<
                System.Collections.IEnumerable>(okResult.Value);

            Assert.Single(enrolments.Cast<object>());
        }

        // TEST 3: A participant should be able to enrol in an available category.
        [Fact]
        public async Task CreateEnrolment_ValidCategory_ReturnsCreatedResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            // Simulate Participant 10 being logged in.
            var controller = CreateController(
                context, 10, "Participant");

            // Prepare the enrolment request.
            var request = new EnrolmentRequest
            {
                CategoryID = 1
            };

            // Act: Create the enrolment.
            var result = await controller.CreateEnrolment(request);

            // Assert: The response should be HTTP 201 Created.
            Assert.IsType<CreatedResult>(result);

            // Assert: The enrolment should be saved.
            Assert.Equal(1, await context.Enrolments.CountAsync());

            var savedEnrolment = await context.Enrolments.FirstAsync();

            Assert.Equal(10, savedEnrolment.ParticipantID);
            Assert.Equal(1, savedEnrolment.CategoryID);
            Assert.Equal("Active", savedEnrolment.Status);
        }

        // TEST 4: Enrolment should fail if the category does not exist.
        [Fact]
        public async Task CreateEnrolment_InvalidCategory_ReturnsNotFound()
        {
            // Arrange: Create an empty database.
            using var context = CreateDatabase();

            var controller = CreateController(
                context, 10, "Participant");

            var request = new EnrolmentRequest
            {
                CategoryID = 999
            };

            // Act: Attempt to enrol in a missing category.
            var result = await controller.CreateEnrolment(request);

            // Assert: The response should be HTTP 404 Not Found.
            Assert.IsType<NotFoundObjectResult>(result);

            // Assert: No enrolment should be saved.
            Assert.Equal(0, await context.Enrolments.CountAsync());
        }

        // TEST 5: A participant should not enrol in the same category twice.
        [Fact]
        public async Task CreateEnrolment_DuplicateEnrolment_ReturnsConflict()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            // Add the participant's existing enrolment.
            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 10,
                CategoryID = 1,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(
                context, 10, "Participant");

            var request = new EnrolmentRequest
            {
                CategoryID = 1
            };

            // Act: Attempt to enrol in the same category again.
            var result = await controller.CreateEnrolment(request);

            // Assert: The response should be HTTP 409 Conflict.
            Assert.IsType<ConflictObjectResult>(result);

            // Assert: The duplicate enrolment should not be saved.
            Assert.Equal(1, await context.Enrolments.CountAsync());
        }

        // TEST 6: Enrolment should fail when the category is full.
        [Fact]
        public async Task CreateEnrolment_FullCategory_ReturnsConflict()
        {
            // Arrange: Create a category with a capacity of one participant.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context, maxParticipants: 1);

            // Fill the category with another participant.
            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 20,
                CategoryID = 1,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(
                context, 10, "Participant");

            var request = new EnrolmentRequest
            {
                CategoryID = 1
            };

            // Act: Attempt to enrol another participant.
            var result = await controller.CreateEnrolment(request);

            // Assert: The response should be HTTP 409 Conflict.
            Assert.IsType<ConflictObjectResult>(result);

            // Assert: The number of enrolments should remain unchanged.
            Assert.Equal(1, await context.Enrolments.CountAsync());
        }

        // TEST 7: A participant should be able to cancel their own enrolment.
        [Fact]
        public async Task CancelEnrolment_OwnEnrolment_ReturnsOkResult()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 10,
                CategoryID = 1,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var controller = CreateController(
                context, 10, "Participant");

            // Act: Cancel the participant's enrolment.
            var result = await controller.CancelEnrolment(1);

            // Assert: The response should be HTTP 200 OK.
            Assert.IsType<OkObjectResult>(result);

            // Assert: The enrolment should be removed.
            Assert.Equal(0, await context.Enrolments.CountAsync());
        }

        // TEST 8: A participant should not cancel another participant's enrolment.
        [Fact]
        public async Task CancelEnrolment_OtherParticipantsEnrolment_ReturnsNotFound()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            await AddSampleEventAndCategory(context);

            // Add an enrolment belonging to Participant 20.
            context.Enrolments.Add(new Enrolment
            {
                EnrolmentID = 1,
                ParticipantID = 20,
                CategoryID = 1,
                EnrolmentDate = DateTime.Today,
                Status = "Active"
            });

            await context.SaveChangesAsync();

            // Simulate Participant 10 being logged in.
            var controller = CreateController(
                context, 10, "Participant");

            // Act: Attempt to cancel Participant 20's enrolment.
            var result = await controller.CancelEnrolment(1);

            // Assert: The response should be HTTP 404 Not Found.
            Assert.IsType<NotFoundObjectResult>(result);

            // Assert: The original enrolment should remain unchanged.
            Assert.Equal(1, await context.Enrolments.CountAsync());
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
