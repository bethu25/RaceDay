using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Data;
using RaceDay.API.Clean.DTOs;
using RaceDay.API.Clean.Models;

namespace RaceDay.API.Clean.Services
{
    // Handles user registration and login verification.
    public class AuthService
    {
        // Provides access to the RaceDay SQL Server database.
        private readonly RaceDayDbContext _context;

    // Provides secure password hashing and verification.
    private readonly IPasswordHasher<User> _passwordHasher;

        // Receives the required dependencies from ASP.NET Core.
        public AuthService(
            RaceDayDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Registers a new user and returns the saved user.
        // Returns null if the email address is already registered.
        public async Task<User?> RegisterAsync(RegisterRequest request)
        {
            // Check whether the email address is already in use.
            bool emailExists = await _context.Users
                .AnyAsync(user => user.Email == request.Email);

            // Stop registration if the email already exists.
            if (emailExists)
            {
                return null;
            }

            // Create the new user using the registration details.
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                Role = request.Role,
                PhoneNumber = request.PhoneNumber.Trim()
            };

            // Hash the password instead of storing the original password.
            user.Password = _passwordHasher.HashPassword(
                user,
                request.Password);

            // Add the new user to the database.
            _context.Users.Add(user);

            // Save the new user to SQL Server.
            await _context.SaveChangesAsync();

            // Return the saved user to the calling code.
            return user;
        }

        // Verifies login credentials and returns the user if valid.
        // Returns null if the email or password is incorrect.
        public async Task<User?> LoginAsync(LoginRequest request)
        {
            // Find the account using the supplied email address.
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    existingUser => existingUser.Email == request.Email);

            // Reject the login if the account does not exist.
            if (user == null)
            {
                return null;
            }

            // Compare the submitted password with the stored password hash.
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                request.Password);

            // Reject the login if the password is incorrect.
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // Return the authenticated user to the calling code.
            return user;
        }
    }

}
