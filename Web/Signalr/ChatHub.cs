using Microsoft.AspNetCore.SignalR;

namespace Web.Signalr;

public sealed class ChatHub(
    ILogger<ChatHub> logger
) : Hub
{
    private readonly ILogger<ChatHub> _logger = logger;

    public Task SendMessage(string message)
    {
        _logger.LogInformation(message);
        return Task.CompletedTask;
    }
}
