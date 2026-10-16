using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.API.Tests
{
    public class ProfileControllerTests
    {
        // Creates a separate in-memory database for each test.
        private static RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // Creates the controller and gives it a logged-in user's session.
        private static ProfileController CreateController(
            RaceDayDbContext context, int userId)
        {
            var controller = new ProfileController(context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            // Store the user's ID so the controller can identify the account.
            httpContext.Session.SetInt32("UserID", userId);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // Creates a sample user for the profile tests.
        private static User CreateUser(
            string fullName, string email, string role = "Participant")
        {
            return new User
            {
                FullName = fullName,
                Email = email,
                Role = role,
                PhoneNumber = "0712345678"
            };
        }

        [Fact]
        public async Task GetProfile_ExistingUser_ReturnsProfile()
        {
            // Arrange: Add a user to the database.
            using var context = CreateDatabase();

            var user = CreateUser("Test Participant", "participant@test.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = CreateController(context, user.UserID);

            // Act: Retrieve the user's profile.
            var response = await controller.GetProfile();

            // Assert: The controller should return the profile successfully.
            var result = Assert.IsType<OkObjectResult>(response);

            Assert.NotNull(result.Value);
            Assert.Equal(user.UserID,
                result.Value.GetType().GetProperty("UserID")!.GetValue(result.Value));
            Assert.Equal("Test Participant",
                result.Value.GetType().GetProperty("FullName")!.GetValue(result.Value));
        }

        [Fact]
        public async Task GetProfile_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange: Create an empty database and request a nonexistent user.
            using var context = CreateDatabase();
            var controller = CreateController(context, 999);

            // Act: Retrieve the profile.
            var response = await controller.GetProfile();

            // Assert: The controller should report that the profile was not found.
            Assert.IsType<NotFoundObjectResult>(response);
        }

        [Fact]
        public async Task UpdateProfile_ValidRequest_UpdatesProfile()
        {
            // Arrange: Add a user to the database.
            using var context = CreateDatabase();

            var user = CreateUser("Old Name", "old@test.com");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = CreateController(context, user.UserID);

            var request = new UpdateProfileRequest
            {
                FullName = "Updated Name",
                Email = "updated@test.com",
                PhoneNumber = "0798765432"
            };

            // Act: Update the profile.
            var response = await controller.UpdateProfile(request);

            // Assert: The update should succeed and the database should contain the changes.
            Assert.IsType<OkObjectResult>(response);

            var updatedUser = await context.Users.FindAsync(user.UserID);

            Assert.NotNull(updatedUser);
            Assert.Equal("Updated Name", updatedUser.FullName);
            Assert.Equal("updated@test.com", updatedUser.Email);
            Assert.Equal("0798765432", updatedUser.PhoneNumber);
        }

        [Fact]
        public async Task UpdateProfile_UserDoesNotExist_ReturnsNotFound()
        {
            // Arrange: Request an update for a user who is not in the database.
            using var context = CreateDatabase();
            var controller = CreateController(context, 999);

            var request = new UpdateProfileRequest
            {
                FullName = "Updated Name",
                Email = "updated@test.com",
                PhoneNumber = "0798765432"
            };

            // Act: Attempt to update the profile.
            var response = await controller.UpdateProfile(request);

            // Assert: The controller should return Not Found.
            Assert.IsType<NotFoundObjectResult>(response);
        }

        [Fact]
        public async Task UpdateProfile_EmailAlreadyUsed_ReturnsConflict()
        {
            // Arrange: Add two users with different email addresses.
            using var context = CreateDatabase();

            var firstUser = CreateUser("First User", "first@test.com");
            var secondUser = CreateUser("Second User", "second@test.com");

            context.Users.AddRange(firstUser, secondUser);
            await context.SaveChangesAsync();

            // The first user tries to use the second user's email address.
            var controller = CreateController(context, firstUser.UserID);

            var request = new UpdateProfileRequest
            {
                FullName = "First User",
                Email = "second@test.com",
                PhoneNumber = "0798765432"
            };

            // Act: Attempt the update.
            var response = await controller.UpdateProfile(request);

            // Assert: The controller should reject the duplicate email address.
            Assert.IsType<ConflictObjectResult>(response);
        }

        [Fact]
        public async Task UpdateProfile_DoesNotChangeUserRole()
        {
            // Arrange: Create an organiser account.
            using var context = CreateDatabase();

            var user = CreateUser(
                "Test Organiser", "organiser@test.com", "Organiser");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = CreateController(context, user.UserID);

            var request = new UpdateProfileRequest
            {
                FullName = "Updated Organiser",
                Email = "organiser.updated@test.com",
                PhoneNumber = "0787654321"
            };

            // Act: Update the organiser's permitted profile details.
            var response = await controller.UpdateProfile(request);

            // Assert: The update succeeds without changing the role.
            Assert.IsType<OkObjectResult>(response);

            var updatedUser = await context.Users.FindAsync(user.UserID);

            Assert.NotNull(updatedUser);
            Assert.Equal("Organiser", updatedUser.Role);
        }

        // Provides a lightweight session for controller testing.
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
