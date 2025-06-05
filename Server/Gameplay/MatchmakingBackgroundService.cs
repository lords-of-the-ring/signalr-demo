using Microsoft.AspNetCore.SignalR;
using Shared;

namespace Server.Gameplay;

public sealed class MatchmakingBackgroundService(
    IWaitingQueue waitingQueue,
    IGameRoomStore gameRoomStore,
    IPlayerStore playerStore,
    IHubContext<GameHub, IGameHubClient> gameHubContext
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10_000, stoppingToken);

            var players = await waitingQueue.GetAllPlayers(stoppingToken);

            var count = players.Length % 2 != 0 ? players.Length - 1 : players.Length;

            if (count == 0)
            {
                continue;
            }

            for (int i = 0; i < count; i += 2)
            {
                var room = new GameRoomState
                {
                    Id = new GameRoomId(Guid.NewGuid().ToString()),
                    CrossPlayer = players[i],
                    NoughtPlayer = players[i + 1]
                };

                await waitingQueue.RemovePlayer(room.CrossPlayer, stoppingToken);
                await waitingQueue.RemovePlayer(room.NoughtPlayer, stoppingToken);

                await gameRoomStore.Add(room, stoppingToken);

                await playerStore.SetGameRoomId(room.CrossPlayer, room.Id, stoppingToken);
                await playerStore.SetGameRoomId(room.NoughtPlayer, room.Id, stoppingToken);

                var crossPlayerConnectionId = await playerStore.GetConnection(room.CrossPlayer, stoppingToken);
                var noughtPlayerConnectionId = await playerStore.GetConnection(room.NoughtPlayer, stoppingToken);

                ArgumentNullException.ThrowIfNull(crossPlayerConnectionId);
                ArgumentNullException.ThrowIfNull(noughtPlayerConnectionId);

                await gameHubContext
                    .Clients
                    .Client(crossPlayerConnectionId.Value)
                    .GameRoomJoined(GameConstants.Cross, room);

                await gameHubContext
                    .Clients
                    .Client(noughtPlayerConnectionId.Value)
                    .GameRoomJoined(GameConstants.Nought, room);
            }
        }
    }
}
