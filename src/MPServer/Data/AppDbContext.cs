using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPServer.Models;

namespace MPServer.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public DbSet<HeartBeat> HeartBeat { get; set; }

        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
