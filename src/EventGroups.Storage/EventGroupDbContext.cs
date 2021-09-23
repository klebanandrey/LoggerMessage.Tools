using EventGroups.Storage.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventGroups.Storage
{
    public class EventGroupDbContext : IdentityDbContext
    {
        public EventGroupDbContext(DbContextOptions<EventGroupDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventGroup> EventGroups { get; set; }

        public DbSet<Solution> Solutions { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(@"Data Source=EventGroups.db");

    }
}
