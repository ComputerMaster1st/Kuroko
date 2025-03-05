using Microsoft.EntityFrameworkCore;

namespace Kuroko.Database;

public static class DatabaseContextExtensions
{
    public static async Task<TDiscordEntity> GetOrCreateRootAsync<TDiscordEntity>(
        this DbSet<TDiscordEntity> dbTable, ulong id) where TDiscordEntity : class, IDiscordEntity
    {
        var data = await dbTable.FirstOrDefaultAsync(x => x.Id == id);

        if (data != null)
            return data;

        data = (TDiscordEntity)Activator.CreateInstance(typeof(TDiscordEntity), id);

        await dbTable.AddAsync(data);
        return data;
    }
}