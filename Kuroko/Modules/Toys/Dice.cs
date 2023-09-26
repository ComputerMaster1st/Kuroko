using Discord.Interactions;
using Kuroko.Core;
using System.Text;

namespace Kuroko.Modules.Toys
{
    public class Dice : KurokoModuleBase
    {
        private readonly Random _random = new();

        [SlashCommand("dice", "Roll the dice!")]
        public async Task ExecuteAsync(int rolls, int diceSize = 6)
        {
            var output = new StringBuilder();
            var total = 0;
            var resultArray = new List<int>();

            if (rolls < 1 || rolls > 100)
            {
                await RespondAsync("Rolls out of range! (Min: 1 | Max: 100)", ephemeral: true);
                return;
            }

            if (diceSize < 2 || diceSize > 100)
            {
                await RespondAsync("Dice size out of range! (Min: 2 | Max: 100)", ephemeral: true);
                return;
            }

            for (int i = 0; i < rolls; i++)
            {
                var result = _random.Next(diceSize) + 1;
                total += result;
                resultArray.Add(result);
            }

            output.AppendJoin(", ", resultArray);
            output.AppendLine()
                .AppendLine("**Total:** " + total);

            await RespondAsync(output.ToString());
        }
    }
}
