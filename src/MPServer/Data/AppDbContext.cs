using Microsoft.EntityFrameworkCore;
using MPServer.Models;
using OpenIddict;

namespace MPServer.Data
{
    public class AppDbContext : OpenIddictDbContext<User>
    {
        public DbSet<HeartBeat> HeartBeat { get; set; }
        public DbSet<Message> Message { get; set; }

        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
