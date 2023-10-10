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

        public static async Task<TGuildPropertyEntity> CreateOrGetPropertiesAsync<TGuildPropertyEntity, TDiscordEntity>(
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

            var discordEntity = await dbDiscordEntity.GetOrCreateRootAsync(guildId);
            guildEntityAction(discordEntity, propertyEntity);

            return propertyEntity;
        }

        public async static Task<TDiscordEntity> GetOrCreateRootAsync<TDiscordEntity>(
            this DbSet<TDiscordEntity> dbTable, ulong Id) where TDiscordEntity : class, IDiscordEntity
        {
            var data = await dbTable.GetDataAsync(Id);

            if (data != null)
                return data;

            data = (TDiscordEntity)Activator.CreateInstance(typeof(TDiscordEntity), Id);

            await dbTable.AddAsync(data);

            return data;
        }

        public static void Add<TDiscordEntity, TPropertyEntity>(this List<TPropertyEntity> ticketMessageList, TPropertyEntity msg, TDiscordEntity root)
            where TPropertyEntity : class, IPropertyEntity
            where TDiscordEntity : class, IDiscordEntity
        {
            var properties = root.GetType().GetProperties();
            var property = properties.FirstOrDefault(x => x.PropertyType == typeof(List<TPropertyEntity>));

            var value = property.GetValue(root) as List<TPropertyEntity>;

            value.Add(msg);
            ticketMessageList.Add(msg);
        }

        public static void Remove<TTypeEntity>(this List<TTypeEntity> entityList, TTypeEntity entity, DatabaseContext db)
            where TTypeEntity : class, ITypeEntity
        {
            entityList.Remove(entity);
            db.Set<TTypeEntity>().Remove(entity);
        }

        public static void Clear<TTypeEntity>(this List<TTypeEntity> entityList, DatabaseContext db)
            where TTypeEntity : class, ITypeEntity
        {
            var temp = new List<TTypeEntity>(entityList);

            entityList.Clear();
            db.Set<TTypeEntity>().RemoveRange(temp);
        }
    }
}
