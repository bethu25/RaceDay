using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RaceDay.API.Clean.Controllers;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;
using RaceDay.API.Clean.Services;
using Xunit;

namespace RaceDay.API.Tests.ControllersTests
{
    public class AuthControllerTests
    {
        // Creates an isolated in-memory database for each test.
        private static RaceDayDbContext CreateDatabase()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        // Creates the authentication service and controller for testing.
        private static AuthController CreateController(
            RaceDayDbContext context)
        {
            // Use ASP.NET Core's password hasher, just like a real application.
            var passwordHasher = new PasswordHasher<User>();

            var authService = new AuthService(context, passwordHasher);

            var controller = new AuthController(authService);

            // Attach a simulated HTTP context so login can use a session.
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // A simple session implementation for unit testing.
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

        // TEST 1: A new user can register successfully.
        [Fact]
        public async Task Register_NewUser_ReturnsCreated()
        {
            using var context = CreateDatabase();

            var controller = CreateController(context);

            var request = new RegisterRequest
            {
                FullName = "Test Runner",
                Email = "runner@example.com",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            var response = await controller.Register(request);

            Assert.IsType<CreatedResult>(response);

            // Confirm that the new account was saved.
            Assert.Equal(1, await context.Users.CountAsync());

            // Confirm that the stored password is hashed, not plain text.
            var savedUser = await context.Users.FirstAsync();

            Assert.NotEqual("TestPassword123!", savedUser.Password);
        }

        // TEST 2: Registering with an existing email returns Conflict.
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            using var context = CreateDatabase();

            var controller = CreateController(context);

            var firstRequest = new RegisterRequest
            {
                FullName = "First Runner",
                Email = "runner@example.com",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            // Register the first account.
            await controller.Register(firstRequest);

            var secondRequest = new RegisterRequest
            {
                FullName = "Second Runner",
                Email = "runner@example.com",
                Password = "AnotherPassword123!",
                Role = "Participant",
                PhoneNumber = "0831234567"
            };

            // Attempt to register another account with the same email.
            var response = await controller.Register(secondRequest);

            Assert.IsType<ConflictObjectResult>(response);

            // Only the first account should exist.
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // TEST 3: A user with correct credentials can log in.
        [Fact]
        public async Task Login_CorrectCredentials_ReturnsOk()
        {
            using var context = CreateDatabase();

            var controller = CreateController(context);

            // Create an account before attempting to log in.
            var registerRequest = new RegisterRequest
            {
                FullName = "Test Runner",
                Email = "runner@example.com",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            await controller.Register(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "runner@example.com",
                Password = "TestPassword123!"
            };

            var response = await controller.Login(loginRequest);

            Assert.IsType<OkObjectResult>(response);
        }

        // TEST 4: Incorrect login details return Unauthorized.
        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            using var context = CreateDatabase();

            var controller = CreateController(context);

            // Register an account first.
            var registerRequest = new RegisterRequest
            {
                FullName = "Test Runner",
                Email = "runner@example.com",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            await controller.Register(registerRequest);

            // Submit an incorrect password.
            var loginRequest = new LoginRequest
            {
                Email = "runner@example.com",
                Password = "WrongPassword123!"
            };

            var response = await controller.Login(loginRequest);

            Assert.IsType<UnauthorizedObjectResult>(response);
        }

        // TEST 5: Successful login stores the user's details in the session.
        [Fact]
        public async Task Login_CorrectCredentials_SetsSessionValues()
        {
            using var context = CreateDatabase();

            var controller = CreateController(context);

            var registerRequest = new RegisterRequest
            {
                FullName = "Test Runner",
                Email = "runner@example.com",
                Password = "TestPassword123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            await controller.Register(registerRequest);

            var loginRequest = new LoginRequest
            {
                Email = "runner@example.com",
                Password = "TestPassword123!"
            };

            await controller.Login(loginRequest);

            // Read the session values saved by the controller.
            Assert.Equal(
                1,
                controller.HttpContext.Session.GetInt32("UserID"));

            Assert.Equal(
                "Participant",
                controller.HttpContext.Session.GetString("UserRole"));

            Assert.Equal(
                "Test Runner",
                controller.HttpContext.Session.GetString("FullName"));
        }
    }
}
