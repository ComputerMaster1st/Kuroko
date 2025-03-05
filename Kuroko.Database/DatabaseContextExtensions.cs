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
    
    public static async Task<TGuildPropertyEntity> CreateOrGetPropertiesAsync<TGuildPropertyEntity, TDiscordEntity>(
        this DbSet<TGuildPropertyEntity> dbProperty, 
        DbSet<TDiscordEntity> dbDiscordEntity, 
        ulong guildId,
        Action<TDiscordEntity, TGuildPropertyEntity> guildEntityAction)
        where TGuildPropertyEntity : class, IPropertyEntity
        where TDiscordEntity : class, IDiscordEntity
    {
        var propertyEntity = await dbProperty.FirstOrDefaultAsync(x => x.GuildId == guildId);

        if (propertyEntity != null)
            return propertyEntity;

        propertyEntity = Activator.CreateInstance<TGuildPropertyEntity>();

        await dbProperty.AddAsync(propertyEntity);

        var discordEntity = await dbDiscordEntity.GetOrCreateDataAsync(guildId);
        guildEntityAction(discordEntity, propertyEntity);

        return propertyEntity;
    }
}