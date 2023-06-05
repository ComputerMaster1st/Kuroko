using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Kuroko.Events
{
    [PreInitialize]
    internal class DiscordSlashCommandEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;

        private IServiceProvider _serviceProvider;

        public DiscordSlashCommandEvent(DiscordShardedClient client, InteractionService commands, IServiceProvider services)
        {
            client.InteractionCreated += (a) => Task.Factory.StartNew(() => Client_InteractionCreated(a));
            commands.InteractionExecuted += (c, ctx, r) => Task.Factory.StartNew(() => Client_SlashCommandExecuted(ctx, r));

            _client = client;
            _interactionService = commands;
            _serviceProvider = services;
        }

        private async Task Client_InteractionCreated(SocketInteraction interaction)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ctx = new KurokoInteractionContext(_client, interaction, scope.ServiceProvider);

                var result = await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Error, "SlashCMD", ex.Message, ex));

                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private static async Task Client_SlashCommandExecuted(IInteractionContext ctx, IResult result)
        {
            if (result.IsSuccess)
                return;

            var output = new StringBuilder();

            switch (result.Error)
            {
                case InteractionCommandError.Exception:
                    var ex = ((ExecuteResult)result).Exception;

                    await Utilities.WriteLogAsync(new LogMessage(
                        LogSeverity.Warning,
                        "SlashCMD",
                        "Slash Command Exception: " + result.ErrorReason,
                        ex
                    ));

                    output.AppendFormat("Error! Please contact developer with following: {0}", ex.Message);
                    break;
                case InteractionCommandError.Unsuccessful:
                    await Utilities.WriteLogAsync(new LogMessage(
                        LogSeverity.Warning,
                        "SlashCMD",
                        "Slash Command Error: " + result.ErrorReason
                    ));

                    output.AppendFormat("Error! {0}", result.ErrorReason);
                    break;
                case InteractionCommandError.UnmetPrecondition:
                    output.AppendFormat("Precondition Error: {0}", result.ErrorReason);
                    break;
                default:
                    await Utilities.WriteLogAsync(new LogMessage(
                        LogSeverity.Warning,
                        "SlashCMD",
                        "Command Error: " + result.ErrorReason
                    ));
                    break;
            }

            await ctx.Interaction.RespondAsync(output.ToString(), ephemeral: true);
        }

        public void RefreshServiceProvider(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;
    }
}
