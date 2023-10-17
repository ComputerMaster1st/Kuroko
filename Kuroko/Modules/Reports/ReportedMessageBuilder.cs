using Discord;

namespace Kuroko.Modules.Reports
{
    public static class ReportedMessageBuilder
    {
        public static Embed Build(string content, DateTimeOffset timestamp)
        {
            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Magenta,
                Timestamp = timestamp,
                Description = content
            };

            return embedBuilder.Build();
        }
    }
}
