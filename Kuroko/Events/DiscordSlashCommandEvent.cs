using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class DiscordSlashCommandEvent
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;

    public DiscordSlashCommandEvent(DiscordShardedClient client, InteractionService interactions, IServiceProvider services)
    {
        _client = client;
        _interactions = interactions;
        _services = services;

        _client.InteractionCreated += (i)
            => Task.Factory.StartNew(()
                => InteractionCreated(i));
        
        _interactions.InteractionExecuted += (_, ctx, r)
            => Task.Factory.StartNew(() 
                => SlashCommandExecuted(ctx, r));
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        try
        {
            using var scope = _services.CreateScope();
            var localServices = scope.ServiceProvider;
            
            var ctx = new KurokoInteractionContext(_client, interaction, localServices);

            _ = await _interactions.ExecuteCommandAsync(ctx, localServices);
        }
        catch (Exception ex)
        {
            await Utilities.WriteLogAsync(
                new LogMessage(
                   LogSeverity.Error,
                   LogHeader.SLASHCMD,
                   ex.Message,
                   ex
            ));

            if (interaction.Type == InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) =>
                        await msg.Result.DeleteAsync()
                    );
        }
    }

    private static async Task SlashCommandExecuted(IInteractionContext ctx, IResult result)
    {
        if (result.IsSuccess)
            return;

        var output = new StringBuilder();

        switch (result.Error)
        {
            case InteractionCommandError.Exception:
                var ex = ((ExecuteResult)result).Exception;

                await Utilities.WriteLogAsync(
                    new LogMessage(
                        LogSeverity.Warning,
                        LogHeader.SLASHCMD,
                        "Slash Command Exception: " + result.ErrorReason,
                        ex
                ));

                output.Append($"Command Exception (Contact NekoTech Support): {ex.Message}");
                break;
            case InteractionCommandError.Unsuccessful:
                await Utilities.WriteLogAsync(
                    new LogMessage(
                        LogSeverity.Warning,
                        LogHeader.SLASHCMD,
                        "Slash Command Error: " + result.ErrorReason
                ));

                output.Append($"Command Unsuccessful: {result.ErrorReason}");
                break;
            case InteractionCommandError.UnmetPrecondition:
                output.Append($"Precondition Error: {result.ErrorReason}");
                break;
            default:
                await Utilities.WriteLogAsync(
                    new LogMessage(
                        LogSeverity.Warning,
                        LogHeader.SLASHCMD,
                        "Command Error: " + result.ErrorReason
                ));
                break;
        }

        if (output.Length > 0)
            await ctx.Interaction.RespondAsync(output.ToString(), ephemeral: true);
    }
}
