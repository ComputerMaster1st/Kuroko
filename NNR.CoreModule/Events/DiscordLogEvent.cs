using Discord;
using Discord.WebSocket;
using NNR.MDK;
using NNR.MDK.Attributes;

namespace NNR.CoreModule.Events
{
    [PreInitialize]
    public class DiscordLogEvent : NnrEvent
    {
        private readonly DiscordShardedClient _client;

        public DiscordLogEvent(DiscordShardedClient discordClient)
        {
            _client = discordClient;
            _client.Log += LogEvent;
        }

        public override void Unload()
            => _client.Log -= LogEvent;

        private Task LogEvent(LogMessage logMessage)
            => Utilities.WriteLogAsync(logMessage);
    }
}
