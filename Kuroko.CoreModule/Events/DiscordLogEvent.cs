using Discord;
using Discord.WebSocket;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;

namespace Kuroko.CoreModule.Events
{
    [KurokoEvent]
    public class DiscordLogEvent : KurokoEvent
    {
        private DiscordShardedClient _client;

        public DiscordLogEvent(DiscordShardedClient discordClient)
        {
            _client = discordClient;
            _client.Log += LogEvent;
        }

        public override void Unload()
        {
            _client.Log -= LogEvent;
            _client = null;
        }

        [KurokoEvent]
        public static Task LogEvent(LogMessage logMessage)
            => Utilities.WriteLogAsync(logMessage);
    }
}
