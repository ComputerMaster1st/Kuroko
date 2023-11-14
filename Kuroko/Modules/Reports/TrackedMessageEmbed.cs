using Discord;

namespace Kuroko.Modules.Reports
{
    public static class TrackedMessageEmbed
    {
        public static Embed Build(string content, DateTimeOffset? timestamp, string title = null)
        {
            var embedBuilder = new EmbedBuilder()
            {
                Title = title ?? "Original Reported Message Content",
                Color = Color.Magenta,
                Timestamp = timestamp,
                Description = content
            };

            return embedBuilder.Build();
        }
    }
}
