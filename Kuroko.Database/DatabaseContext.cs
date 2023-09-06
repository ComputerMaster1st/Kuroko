using Kuroko.Database.Entities.Guild;
using Kuroko.Database.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database
{
    public sealed class DatabaseContext : DbContext
    {
        // Root containers. Should only contain foreign keys
        public DbSet<GuildEntity> Guilds { get; set; } = null;
        public DbSet<UserEntity> Users { get; set; } = null;

        // TODO: Put Module DbSets Here

        public DbSet<RoleRequestEntity> GuildRoleRequests { get; set; } = null;

        // END

        public DatabaseContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Setup Relations for your modules here

            #region RoleRequest

            modelBuilder.Entity<RoleRequestEntity>()
                .HasMany(x => x.RoleIds)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GuildEntity>()
                .HasOne(x => x.RoleRequest)
                .WithOne(x => x.Guild)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            SaveChanges();
            base.Dispose();
        }
    }
}