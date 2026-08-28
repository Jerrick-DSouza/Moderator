using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

class Program
{
    private DiscordSocketClient _client;
    private InteractionService _commands;
    private IServiceProvider _services = null;

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        string configJson = File.ReadAllText("config.json");
        using var doc = JsonDocument.Parse(configJson);
        string token = doc.RootElement.GetProperty("Token").GetString();

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _commands = new InteractionService(_client.Rest);

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteractionAsync;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected and online!");

        ulong guildId = 1540419890278572145;
        await _commands.RegisterCommandsToGuildAsync(guildId);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _commands.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                Console.WriteLine($"Command execution failed: {result.ErrorReason}");
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync();
        }
    }
}

public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("kick", "Kicks a member from the server.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    public async Task KickAsync(
        [Summary("user", "The user to kick")] SocketGuildUser user,
        [Summary("reason", "The reason for the kick")] string reason = "No reason provided")
    {
        await user.KickAsync(reason);
        await RespondAsync($"Successfully kicked {user.Mention}. Reason: {reason}", ephemeral: true);
    }

    [SlashCommand("ban", "Bans a member from the server.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task BanAsync(
        [Summary("user", "The user to ban")] SocketGuildUser user,
        [Summary("reason", "The reason for the ban")] string reason = "No reason provided")
    {
        await user.BanAsync(reason: reason);
        await RespondAsync($"Successfully banned {user.Mention}. Reason: {reason}", ephemeral: true);
    }

    [SlashCommand("timeout", "Times out a member for a specified number of minutes.")]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    public async Task TimeoutAsync(
        [Summary("user", "The user to timeout")] SocketGuildUser user,
        [Summary("minutes", "Duration of the timeout in minutes")] int minutes,
        [Summary("reason", "The reason for the timeout")] string reason = "No reason provided")
    {
        await user.SetTimeOutAsync(TimeSpan.FromMinutes(minutes), new RequestOptions { AuditLogReason = reason });
        await RespondAsync($"Successfully timed out {user.Mention} for {minutes} minutes. Reason: {reason}", ephemeral: true);
    }

    [SlashCommand("unban", "Unbans a user from the server.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    public async Task UnbanAsync(
        [Summary("user", "The user to unban"), Autocomplete(typeof(BanAutocompleteHandler))] string userId,
        [Summary("reason", "The reason for the unban")] string reason = "No reason provided")
    {
        if (ulong.TryParse(userId, out ulong id))
        {
            await Context.Guild.RemoveBanAsync(id, new RequestOptions { AuditLogReason = reason });
            await RespondAsync($"Successfully unbanned user ID {id}. Reason: {reason}", ephemeral: true);
        }
        else
        {
            await RespondAsync("Invalid user selection.", ephemeral: true);
        }
    }

    [SlashCommand("untimeout", "Removes the timeout from a member.")]
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [RequireBotPermission(GuildPermission.ModerateMembers)]
    public async Task UntimeoutAsync(
        [Summary("user", "The user to remove timeout from")] SocketGuildUser user,
        [Summary("reason", "The reason for removing the timeout")] string reason = "No reason provided")
    {
        await user.RemoveTimeOutAsync(new RequestOptions { AuditLogReason = reason });
        await RespondAsync($"Successfully removed timeout from {user.Mention}. Reason: {reason}", ephemeral: true);
    }

    public class BanAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            if (context.Guild == null) return AutocompletionResult.FromSuccess();

            var bans = await context.Guild.GetBansAsync().FlattenAsync();

            var results = new List<AutocompleteResult>();

            foreach (var ban in bans)
            {
                string playerName = $"{ban.User.Username} ({ban.User.Id})";
                results.Add(new AutocompleteResult(playerName, ban.User.Id.ToString()));
            }

            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}