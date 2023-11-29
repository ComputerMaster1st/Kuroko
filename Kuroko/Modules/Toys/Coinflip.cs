using Discord;
using Discord.Interactions;
using Kuroko.Core;

namespace Kuroko.Modules.Toys
{
    public class Coinflip : KurokoModuleBase
    {
        private readonly Random _random = new();

        [SlashCommand("coinflip", "Flip a coin.")]
        public async Task ExecuteAsync()
        {
            var embed = new EmbedBuilder()
            {
                Color = Color.Red,
                Title = "Coin"
            };

            var result = _random.Next(2);

            if (result == 1)
                embed.WithDescription("You Flipped.. HEADS!")
                    .WithImageUrl("https://i.imgur.com/Y77AMLp.png");
            else
                embed.WithDescription("You Flipped.. TAILS!")
                    .WithImageUrl("https://i.imgur.com/O3ULvhg.png");
            
            await RespondAsync(embed: embed.Build());
        }
    }
}
