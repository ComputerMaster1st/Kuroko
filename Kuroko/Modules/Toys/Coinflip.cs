using Discord;
using Discord.Interactions;
using Kuroko.Core;
using System.Text;

namespace Kuroko.Modules.Toys
{
    public class Coinflip : KurokoModuleBase
    {
        private readonly Random _random = new();

        [SlashCommand("Coinflip", "Flip a coin.")]
        public async Task ExecuteAsync(int rolls, int diceSize = 6)
        {
            EmbedBuilder embed = new EmbedBuilder()
                 .WithColor(Color.Red)
                 .WithTitle("Coin");

            var result = _random.Next(2);

            if (result == 1)
            {
                embed.WithDescription("You Flipped.. HEADS!");
                embed.WithImageUrl("https://i.imgur.com/Y77AMLp.png");
            }
            else
            {
                embed.WithDescription("You Flipped.. TAILS!");
                embed.WithImageUrl("https://i.imgur.com/O3ULvhg.png");
            }
            
            await ReplyAsync(embed: embed.Build());
            
                
        }
    }
}
