using Kuroko.Database.GuildEntities;
using Kuroko.Database.GuildEntities.Extras;
using Kuroko.Database.UserEntities;
using Kuroko.Database.UserEntities.Extras;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database;

public sealed class DatabaseContext : DbContext
{
    // Base Entities
    public DbSet<GuildEntity> Guilds { get; internal set; } = null;
    public DbSet<UserEntity> Users { get; internal set; } = null;
    
    // Property Entities
    public DbSet<BanSyncGuildProperties> BanSyncProperties { get; internal set; } = null;
    public DbSet<PatreonProperties> PatreonProperties { get; internal set; } = null;
    
    // Extra Entities
    public DbSet<BanSyncProfile> BanSyncProfiles { get; internal set; } = null;
    public DbSet<PremiumKey> PremiumKeys { get; internal set; } = null;
    
#if DEBUG
    public DatabaseContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = KurokoConfig.LoadAsync()
            .GetAwaiter()
            .GetResult();

        optionsBuilder.UseNpgsql(config.ConnectionString())
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies();
        
        base.OnConfiguring(optionsBuilder);
    }
#else
    public DatabaseContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }
#endif

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region BanSyncProperties
        modelBuilder.Entity<BanSyncGuildProperties>()
            .HasMany(x => x.HostForProfiles)
            .WithOne(x => x.HostGuildProperties)
            .HasForeignKey(x => x.HostSyncId)
            .HasPrincipalKey(x => x.SyncId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BanSyncGuildProperties>()
            .HasMany(x => x.ClientOfProfiles)
            .WithOne(x => x.ClientGuildProperties)
            .HasForeignKey(x => x.ClientSyncId)
            .HasPrincipalKey(x => x.SyncId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GuildEntity>()
            .HasOne(x => x.BanSyncGuildProperties)
            .WithOne(x => x.Guild)
            .HasForeignKey<BanSyncGuildProperties>(x => x.RootId)
            .HasPrincipalKey<GuildEntity>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
        
        #region PatreonProperties
        modelBuilder.Entity<PatreonProperties>()
            .HasMany(x => x.PremiumKeys)
            .WithOne(x => x.PatreonProperties)
            .HasForeignKey(x => x.PatreonPropertiesId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PremiumKey>()
            .HasOne(x => x.Guild)
            .WithOne(x => x.PremiumKey)
            .HasForeignKey<PremiumKey>(x => x.GuildId)
            .HasPrincipalKey<GuildEntity>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserEntity>()
            .HasOne(x => x.Patreon)
            .WithOne(x => x.User)
            .HasForeignKey<PatreonProperties>(x => x.RootId)
            .HasPrincipalKey<UserEntity>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
        #endregion
        
        base.OnModelCreating(modelBuilder);
    }

    public override void Dispose()
    {
        SaveChanges();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await SaveChangesAsync();
        await base.DisposeAsync();
    }
}