using Kuroko.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database
{
    public static class DatabaseContextExtensions
    {
        public static Task<TDiscordEntity> GetDataAsync<TDiscordEntity>(this DbSet<TDiscordEntity> dbSet, ulong id) where TDiscordEntity : class, IDiscordEntity
            => dbSet.FirstOrDefaultAsync(x => x.Id == id);

        public static async Task<TDiscordEntity> CreateOrGetDataAsync<TDiscordEntity>(this DbSet<TDiscordEntity> dbSet, ulong id) where TDiscordEntity : class, IDiscordEntity
        {
            var entity = await dbSet.FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
                return entity;

            entity = (TDiscordEntity)Activator.CreateInstance(typeof(TDiscordEntity), id);

            await dbSet.AddAsync(entity);

            return entity;
        }
    }
}
