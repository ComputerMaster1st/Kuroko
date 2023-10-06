using Kuroko.Database.Entities;
using Kuroko.Database.Entities.Guild;
using Kuroko.Database.Entities.Message;
using Kuroko.Database.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database
{
    public sealed class DatabaseContext : DbContext
    {
        // INTERNAL USE ONLY! Use extension methods provided by DatabaseContextExtensions.cs
        internal DbSet<UlongEntity> UlongEntity { get; set; } = null;
        internal DbSet<ReportHandler> ReportHandler { get; set; } = null;
        internal DbSet<AttachmentEntity> AttachmentEntity { get; set; } = null;
        internal DbSet<EditedMessageEntity> EditedMessageEntity { get; set; } = null;

        // Root containers. Should only contain foreign keys
        public DbSet<GuildEntity> Guilds { get; internal set; } = null;
        public DbSet<UserEntity> Users { get; internal set; } = null;
        public DbSet<MessageEntity> Messages { get; set; } = null;

        // TODO: Put Module DbSets Here

        public DbSet<RoleRequestEntity> GuildRoleRequests { get; internal set; } = null;
        public DbSet<ModLogEntity> GuildModLogs { get; internal set; } = null;
        public DbSet<ReportsEntity> GuildReports { get; internal set; } = null;
        public DbSet<TicketEntity> Tickets { get; internal set; } = null;

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

            #region Reports

            modelBuilder.Entity<ReportsEntity>()
                .HasMany(x => x.ReportHandlers)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuildEntity>()
                .HasOne(x => x.Reports)
                .WithOne(x => x.Guild)
                .HasForeignKey<ReportsEntity>(x => x.GuildId)
                .HasPrincipalKey<GuildEntity>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Messages

            modelBuilder.Entity<MessageEntity>()
                .HasMany(x => x.EditedMessages)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MessageEntity>()
                .HasMany(x => x.Attachments)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuildEntity>()
                .HasMany(x => x.Messages)
                .WithOne(x => x.Guild)
                .HasForeignKey(x => x.GuildId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Tickets

            modelBuilder.Entity<TicketEntity>()
                .HasMany(x => x.Messages)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuildEntity>()
                .HasMany(x => x.Tickets)
                .WithOne(x => x.Guild)
                .HasForeignKey(x => x.GuildId)
                .HasPrincipalKey(x => x.Id)
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