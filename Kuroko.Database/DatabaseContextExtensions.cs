using Kuroko.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database
{
    public static class DatabaseContextExtensions
    {
        public static Task<TDiscordEntity> GetDataAsync<TDiscordEntity>(
            this DbSet<TDiscordEntity> dbTable, ulong id)
            where TDiscordEntity : class, IDiscordEntity
            => dbTable.FirstOrDefaultAsync(x => x.Id == id);

        public static async Task<TGuildPropertyEntity> CreateOrGetDataAsync<TGuildPropertyEntity, TDiscordEntity>(
            this DbSet<TGuildPropertyEntity> dbProperty, DbSet<TDiscordEntity> dbDiscordEntity, ulong guildId,
            Action<TDiscordEntity, TGuildPropertyEntity> guildEntityAction)
            where TGuildPropertyEntity : class, IPropertyEntity
            where TDiscordEntity : class, IDiscordEntity
        {
            var propertyEntity = await dbProperty.FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (propertyEntity != null)
                return propertyEntity;

            propertyEntity = (TGuildPropertyEntity)Activator.CreateInstance(typeof(TGuildPropertyEntity));

            await dbProperty.AddAsync(propertyEntity);

            var discordEntity = await dbDiscordEntity.CreateDataAsync(guildId);
            guildEntityAction(discordEntity, propertyEntity);

            return propertyEntity;
        }

        private async static Task<TDiscordEntity> CreateDataAsync<TDiscordEntity>(
            this DbSet<TDiscordEntity> dbTable, ulong Id) where TDiscordEntity : class, IDiscordEntity
        {
            var data = await dbTable.GetDataAsync(Id);

            if (data != null)
                return data;

            data = (TDiscordEntity)Activator.CreateInstance(typeof(TDiscordEntity), Id);

            await dbTable.AddAsync(data);

            return data;
        }
    }
}
