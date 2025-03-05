using Kuroko.Database.GuildEntities;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database;

public sealed class DatabaseContext : DbContext
{
    // Base Entities
    public DbSet<GuildEntity> Guilds { get; internal set; } = null;
    
#if DEBUG
    public DatabaseContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var config = KurokoConfig.LoadAsync()
            .GetAwaiter()
            .GetResult();

        optionsBuilder.UseNpgsql(config.ConnectionString)
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies();
    }
#else
    public DatabaseContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }
#endif

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