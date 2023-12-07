using Discord.Interactions;
using Kuroko.Database;
using Kuroko.Database.Entities;

namespace Kuroko.Core
{
    public abstract class KurokoModuleBase : InteractionModuleBase<KurokoInteractionContext>
    {
        protected async Task<bool> IsInteractedUserAsync(ulong userId)
        {
            if (userId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return false;
            }

            return true;
        }

        protected async Task<TPropertyEntity> GetPropertiesAsync<TPropertyEntity, TDiscordEntity>(ulong rootId)
            where TPropertyEntity : class, IPropertyEntity
            where TDiscordEntity : class, IDiscordEntity
        {
            var set = Context.Database.Set<TPropertyEntity>();
            var rootSet = Context.Database.Set<TDiscordEntity>();
            var result = await set.CreateOrGetPropertiesAsync(rootSet, rootId, (x, y) =>
            {
                var properties = x.GetType().GetProperties();
                var property = properties.FirstOrDefault(x => x.PropertyType == typeof(TPropertyEntity));

                property.SetValue(x, y);
            });

            return result;
        }
    }
}
