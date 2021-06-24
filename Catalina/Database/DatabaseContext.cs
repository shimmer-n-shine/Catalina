using Catalina.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Catalina.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<GuildProperty> GuildProperties { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        public async Task AddReactionAsync(Reaction reaction)
        {
            Reactions.Add(reaction);
            await SaveChangesAsync();
        }
        public async Task RemoveReactionAsync(Reaction reaction)
        {
            Reactions.Remove(reaction);
            await SaveChangesAsync();
        }

    }
}
