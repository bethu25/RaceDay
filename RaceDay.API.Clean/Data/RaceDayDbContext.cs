using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Models;

namespace RaceDay.API.Clean.Data
{
    // Manages communication between the RaceDay API and SQL Server.
    public class RaceDayDbContext : DbContext
    {
        // Receives the database configuration from ASP.NET Core.
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {

        }

        // Represents the Users table in the RaceDay database.
        public DbSet<User> Users { get; set; }
    }
}