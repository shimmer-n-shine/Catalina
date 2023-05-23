using Catalina.Database.Models;
using Microsoft.EntityFrameworkCore;
using static Catalina.Database.Models.StarboardSettings;

namespace Catalina.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions) 
    {
        this.ChangeTracker.LazyLoadingEnabled = true;
    }
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Message> StarboardMessages { get; set; }
    public DbSet<StarboardSettings> StarboardSettings { get; set; }
    public DbSet<Emoji> Emojis { get; set; }
    public DbSet<Vote> StarboardVotes { get; set; }
    public DbSet<TimezoneSettings> TimezonesSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Guild>().HasMany(g => g.Responses);
        modelBuilder.Entity<Guild>().HasMany(g => g.Roles);
        modelBuilder.Entity<Guild>().HasOne(g => g.TimezoneSettings);
        modelBuilder.Entity<Guild>().HasOne(g => g.StarboardSettings);

        modelBuilder.Entity<StarboardSettings>().HasMany(s => s.Messages);
        modelBuilder.Entity<StarboardSettings>().HasOne(s => s.Emoji);

        modelBuilder.Entity<Message>().HasMany(m => m.Votes);

        modelBuilder.Entity<Role>().HasOne(g => g.Guild);

    }
}
