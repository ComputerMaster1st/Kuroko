using Discord.Interactions;
using Kuroko.Core;
using System.Text;

namespace Kuroko.Modules.Toys
{
    public class Dice : KurokoModuleBase
    {
        private readonly Random _random = new();

        [SlashCommand("dice", "Roll the dice!")]
        public Task ExecuteAsync(int rolls, int diceSize = 100)
        {
            var output = new StringBuilder();

            for (int i = 0; i < rolls; i++)
                output.AppendFormat("{0}, ", _random.Next(diceSize) + 1);

            if (diceSize < 1)
                return RespondAsync("Max Dice Number must be 1 or larger!", ephemeral: true);

            return RespondAsync(output.ToString().TrimEnd(','));
        }
    }
}
