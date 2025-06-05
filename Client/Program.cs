using Client;
using Microsoft.AspNetCore.SignalR.Client;
using Shared;

Console.WriteLine("What is your name?");
var username = Console.ReadLine()?.Trim();
ArgumentNullException.ThrowIfNull(username);

Console.WriteLine("Waiting for server to become available...");

var iAmOnTurn = false;

var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5123/hubs/game", options =>
    {
        options.AccessTokenProvider = async () =>
        {
            return await AccessTokenProvider.GetAccessToken(username);
        };
    })
    .WithAutomaticReconnect()
    .Build();

hubConnection.On("ForceDisconnect", () =>
{
    PrintByServer("Disconnected");
    Environment.Exit(0);
});

hubConnection.On("WaitingQueueJoined", () => PrintByServer("Waiting queue joined"));

hubConnection.On("WaitingQueueExited", () => PrintByServer("Waiting queue exited"));

hubConnection.On("GameRoomJoined", (char me, GameRoomState room) =>
{
    iAmOnTurn = room.CurrentSymbol == me;
    PrintByServer($"Game room '{room.Id.Value}' joined against '{GetOpponentName(me, room)}'");
    PrintGameRoomState(me, room);
});

hubConnection.On("RejoinGameRoom", (char me, GameRoomState room) =>
{
    iAmOnTurn = room.CurrentSymbol == me;
    PrintByServer($"Rejoined game room '{room.Id.Value}' against '{GetOpponentName(me, room)}'");
    PrintGameRoomState(me, room);
});

hubConnection.On("GameRoomStateUpdated", (char me, GameRoomState room) =>
{
    iAmOnTurn = room.CurrentSymbol == me;
    PrintByServer("Next turn played");
    PrintGameRoomState(me, room);
});

try
{
    await hubConnection.StartAsync();

    Console.WriteLine("Start processing messages...");

    while (true)
    {
        var commandArgs = Console.ReadLine()!
            .ToLower()
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!iAmOnTurn)
        {
            Console.WriteLine("You are not on turn!");
            continue;
        }

        var command = commandArgs[0];

        if (command == "join-queue")
        {
            await hubConnection.SendAsync("JoinWaitingQueue", CancellationToken.None);
            continue;
        }

        if (command == "exit-queue")
        {
            await hubConnection.SendAsync("ExitWaitingQueue", CancellationToken.None);
            continue;
        }

        if (command == "play")
        {
            int row = int.Parse(commandArgs[1]);
            int column = int.Parse(commandArgs[2]);
            await hubConnection.SendAsync("PlayTurn", row, column, CancellationToken.None);
            continue;
        }
    }
}
finally
{
    if (hubConnection.State == HubConnectionState.Connected ||
        hubConnection.State == HubConnectionState.Reconnecting)
    {
        await hubConnection.StopAsync();
    }

    await hubConnection.DisposeAsync();
}

static void PrintByServer(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"=> {message}");
    Console.ForegroundColor = ConsoleColor.White;
}

static void PrintGameRoomState(char me, GameRoomState room)
{
    if (me == room.CurrentSymbol)
    {
        Console.WriteLine("You are on turn");
    }
    else
    {
        Console.WriteLine($"{GetOpponentName(me, room)} is on turn");
    }

    foreach (var row in room.Board)
    {
        Console.Write("|");

        foreach (var cell in row)
        {
            var formattedCell = cell == GameConstants.EmptyCell ? ' ' : cell;
            Console.Write(formattedCell);
            Console.Write("|");
        }

        Console.WriteLine();
    }
}

static string GetOpponentName(char me, GameRoomState room)
{
    return me == GameConstants.Cross ? room.NoughtPlayer.Value : room.CrossPlayer.Value;
}
