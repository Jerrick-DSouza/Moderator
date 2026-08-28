using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class Program
{
    private DiscordSocketClient _client;

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        string token = "MTU0Mjg5MjMxODcyMDI3MDMzNg.GysvxN.ofOs0KKXRT4T91FV57LZSW99qwd2L1-X1x864o";

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected and online!");
        return Task.CompletedTask;
    }
}