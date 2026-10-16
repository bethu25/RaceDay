using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using RaceDay.API.Clean.Services;
using Xunit;

namespace RaceDay.API.Tests.ServicesTests
{
    public class AuthServiceTests
    {
        // Creates a separate temporary database for each test.
        private RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // TEST 1: Check that a new user can register successfully.
        [Fact]
        public async Task RegisterAsync_NewUser_ReturnsSavedUser()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Create the password hasher.
            var passwordHasher = new PasswordHasher<User>();

            // Arrange: Create the authentication service.
            var authService = new AuthService(context, passwordHasher);

            // Arrange: Prepare the new user's registration details.
            var request = new RegisterRequest
            {
                FullName = "Test Participant",
                Email = "participant@raceday.test",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0123456789"
            };

            // Act: Register the new user.
            var result = await authService.RegisterAsync(request);

            // Assert: A user should be returned.
            Assert.NotNull(result);

            // Assert: The user's details should be saved correctly.
            Assert.Equal("Test Participant", result.FullName);
            Assert.Equal("participant@raceday.test", result.Email);
            Assert.Equal("Participant", result.Role);

            // Assert: The password should not be stored as plain text.
            Assert.NotEqual("TestPassword123!", result.Password);

            // Assert: The user should be saved in the database.
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 2: Check that duplicate email addresses are rejected.
        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsNull()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Add a user who is already registered.
            var existingUser = new User
            {
                FullName = "Existing Participant",
                Email = "participant@raceday.test",
                Password = "ExistingPasswordHash",
                Role = "Participant",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            // Arrange: Create the authentication service.
            var authService = new AuthService(
                context,
                new PasswordHasher<User>());

            // Arrange: Prepare a registration request using the same email.
            var request = new RegisterRequest
            {
                FullName = "Another Participant",
                Email = "participant@raceday.test",
                Password = "AnotherPassword123!",
                Role = "Participant",
                PhoneNumber = "0987654321"
            };

            // Act: Attempt to register the duplicate email.
            var result = await authService.RegisterAsync(request);

            // Assert: Registration should be rejected.
            Assert.Null(result);

            // Assert: No second user should be added.
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 3: Check that a user can log in with the correct password.
        [Fact]
        public async Task LoginAsync_CorrectPassword_ReturnsUser()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Create the password hasher.
            var passwordHasher = new PasswordHasher<User>();

            // Arrange: Create a test user.
            var user = new User
            {
                FullName = "Test Participant",
                Email = "participant@raceday.test",
                Role = "Participant",
                PhoneNumber = "0123456789"
            };

            // Hash the password before saving the user.
            user.Password = passwordHasher.HashPassword(
                user,
                "TestPassword123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Arrange: Create the authentication service.
            var authService = new AuthService(context, passwordHasher);

            // Arrange: Prepare the login details.
            var request = new LoginRequest
            {
                Email = "participant@raceday.test",
                Password = "TestPassword123!"
            };

            // Act: Attempt to log in.
            var result = await authService.LoginAsync(request);

            // Assert: The correct user should be returned.
            Assert.NotNull(result);
            Assert.Equal("participant@raceday.test", result.Email);
        }

        // TEST 4: Check that an incorrect password is rejected.
        [Fact]
        public async Task LoginAsync_IncorrectPassword_ReturnsNull()
        {
            // Arrange: Create the temporary database.
            using var context = CreateDatabase();

            // Arrange: Create the password hasher.
            var passwordHasher = new PasswordHasher<User>();

            // Arrange: Create a test user.
            var user = new User
            {
                FullName = "Test Participant",
                Email = "participant@raceday.test",
                Role = "Participant",
                PhoneNumber = "0123456789"
            };

            // Store a hash of the correct password.
            user.Password = passwordHasher.HashPassword(
                user,
                "CorrectPassword123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Arrange: Create the authentication service.
            var authService = new AuthService(context, passwordHasher);

            // Arrange: Supply an incorrect password.
            var request = new LoginRequest
            {
                Email = "participant@raceday.test",
                Password = "WrongPassword123!"
            };

            // Act: Attempt to log in.
            var result = await authService.LoginAsync(request);

            // Assert: The login should be rejected.
            Assert.Null(result);
        }

        // TEST 5: Check that login fails when the email is not registered.
        [Fact]
        public async Task LoginAsync_UnknownEmail_ReturnsNull()
        {
            // Arrange: Create an empty temporary database.
            using var context = CreateDatabase();

            // Arrange: Create the authentication service.
            var authService = new AuthService(
                context,
                new PasswordHasher<User>());

            // Arrange: Prepare login details for an unknown user.
            var request = new LoginRequest
            {
                Email = "unknown@raceday.test",
                Password = "TestPassword123!"
            };

            // Act: Attempt to log in.
            var result = await authService.LoginAsync(request);

            // Assert: The login should be rejected.
            Assert.Null(result);
        }
    }
}