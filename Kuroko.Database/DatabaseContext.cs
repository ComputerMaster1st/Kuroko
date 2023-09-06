using Kuroko.Database.Entities.Guild;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<GuildEntity> Guilds { get; set; } = null;

        public DatabaseContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            SaveChanges();
            base.Dispose();
        }
    }
}