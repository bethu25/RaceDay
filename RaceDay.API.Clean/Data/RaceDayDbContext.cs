using Microsoft.EntityFrameworkCore;

namespace RaceDay.API.Clean.Data
{
    public class RaceDayDbContext : DbContext // This is the Entity Framework Core class that manages database access
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) //This allows us to configure the database connection.
        {

        }
    }
}
