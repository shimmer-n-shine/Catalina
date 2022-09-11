using Catalina.Database.Models;
using Microsoft.EntityFrameworkCore;
using static Catalina.Database.Models.Starboard;

namespace Catalina.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
        public DbSet<GuildProperty> GuildProperties { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Message> StarboardMessages { get; set; }
        public DbSet<Emoji> Emojis { get; set; }
        public DbSet<Vote> StarboardVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildProperty>().HasMany(g => g.Responses);
            modelBuilder.Entity<GuildProperty>().HasMany(g => g.Roles);

            modelBuilder.Entity<GuildProperty>().HasOne(g => g.Starboard);

            modelBuilder.Entity<Starboard>().HasMany(s => s.Messages);
            modelBuilder.Entity<Starboard>().HasOne(s => s.Emoji);

            modelBuilder.Entity<Message>().HasMany(m => m.Votes);
        }
    }
}
