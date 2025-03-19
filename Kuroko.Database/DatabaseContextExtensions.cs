using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database;

public static class DatabaseContextExtensions
{
    public static async Task<TDiscordEntity> GetOrCreateDataAsync<TDiscordEntity>(
        this DbSet<TDiscordEntity> dbTable, ulong id) where TDiscordEntity : class, IDiscordEntity
    {
        var data = await dbTable.FirstOrDefaultAsync(x => x.Id == id);

        if (data != null)
            return data;

        data = (TDiscordEntity)Activator.CreateInstance(typeof(TDiscordEntity), id);

        await dbTable.AddAsync(data);
        return data;
    }
    
    public static async Task<TPropertyEntity> CreateOrGetPropertiesAsync<TPropertyEntity, TDiscordEntity>(
        this DbSet<TPropertyEntity> dbProperty, 
        DbSet<TDiscordEntity> dbDiscordEntity, 
        ulong rootId,
        Action<TDiscordEntity, TPropertyEntity> guildEntityAction)
        where TPropertyEntity : class, IPropertyEntity
        where TDiscordEntity : class, IDiscordEntity
    {
        var propertyEntity = await dbProperty.FirstOrDefaultAsync(x => x.RootId == rootId);

        if (propertyEntity != null)
            return propertyEntity;

        propertyEntity = Activator.CreateInstance<TPropertyEntity>();

        await dbProperty.AddAsync(propertyEntity);

        var discordEntity = await dbDiscordEntity.GetOrCreateDataAsync(rootId);
        guildEntityAction(discordEntity, propertyEntity);

        return propertyEntity;
    }
}