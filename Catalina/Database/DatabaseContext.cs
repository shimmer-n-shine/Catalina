using Catalina.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalina.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
        public DbSet<GuildProperty> GuildProperties { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StarboardMessage> StarboardMessages { get; set; }
        public DbSet<Emoji> Emojis { get; set; }

        public DbSet<StarboardVote> StarboardVotes { get; set; }

    }
}
