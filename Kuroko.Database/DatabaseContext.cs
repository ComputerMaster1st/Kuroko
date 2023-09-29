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
        public DbSet<ModLogEntity> GuildModLogs { get; set; } = null;

#if DEBUG
        public DatabaseContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var config = Shared.Configuration.KDatabaseConfig.LoadAsync().GetAwaiter().GetResult();

            optionsBuilder.UseNpgsql(config.ConnectionUrl())
                .EnableSensitiveDataLogging()
                .UseLazyLoadingProxies();
        }
#else

        public DatabaseContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }
#endif

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
                .HasForeignKey<RoleRequestEntity>(x => x.GuildId)
                .HasPrincipalKey<GuildEntity>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region ModLogs

            modelBuilder.Entity<ModLogEntity>()
                .HasMany(x => x.IgnoredChannelIds)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuildEntity>()
                .HasOne(x => x.ModLog)
                .WithOne(x => x.Guild)
                .HasForeignKey<ModLogEntity>(x => x.GuildId)
                .HasPrincipalKey<GuildEntity>(x => x.Id)
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