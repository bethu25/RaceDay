using Microsoft.EntityFrameworkCore;
using RaceDay.API.Clean.Models;

namespace RaceDay.API.Clean.Data
{
        // Manages communication between the RaceDay API and SQL Server.
        public class RaceDayDbContext : DbContext
        {
        // Receives the database connection configuration.
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {

        }

        // Represents the Users table.
        public DbSet<User> Users { get; set; }

        // Represents the Events table.
        public DbSet<Event> Events { get; set; }

        // Represents the Categories table.
        public DbSet<Category> Categories { get; set; }

        // Represents the Enrolments table.
        public DbSet<Enrolment> Enrolments { get; set; }

        // Represents the Results table.
        public DbSet<Result> Results { get; set; }

        // Represents the Routes table using our RaceDay Route model.
        public DbSet<RaceDay.API.Clean.Models.Route> Routes { get; set; }


    }

}
