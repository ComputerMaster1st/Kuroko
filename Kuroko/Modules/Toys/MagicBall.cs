using Discord.Interactions;
using Kuroko.Core;
using System.Text;

namespace Kuroko.Modules.Toys
{
    public class MagicBall : KurokoModuleBase
    {
        private readonly Random _random = new();

        private readonly string[] _responses = {
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            "Reply hazy try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful."
        };

        [SlashCommand("8ball", "Magic 8 Ball!")]
        public Task ExecuteAsync(string query = "")
        {
            var output = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(query))
                output.AppendFormat("Your Question: {0}", query).AppendLine();

            output.AppendFormat("Magic 8 Ball's Response: {0}",
                _responses[_random.Next(0, _responses.Length)]);

            return RespondAsync(output.ToString());
        }
    }
}
