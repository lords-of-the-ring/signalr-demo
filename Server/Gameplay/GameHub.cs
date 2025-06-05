using Microsoft.AspNetCore.SignalR;
using Shared;

namespace Server.Gameplay;

public sealed class GameHub(
    IPlayerStore playerStore,
    IGameRoomStore gameRoomStore,
    IWaitingQueue waitingQueue
) : Hub<IGameHubClient>
{
    public PlayerId CurrentPlayerId
    {
        get
        {
            var claim = Context.UserIdentifier;
            ArgumentNullException.ThrowIfNull(claim);
            return new PlayerId(claim);
        }
    }

    public ConnectionId CurrentConnectionId => new(Context.ConnectionId);

    public override async Task OnConnectedAsync()
    {
        var oldConnectionId = await playerStore.SetConnection(
            CurrentPlayerId,
            CurrentConnectionId,
            CancellationToken.None);

        if (oldConnectionId is not null)
        {
            await Clients.Client(oldConnectionId.Value).ForceDisconnect();
        }

        var gameRoomId = await playerStore.GetGameRoomId(CurrentPlayerId, CancellationToken.None);

        if (gameRoomId is not null)
        {
            var room = await gameRoomStore.Get(gameRoomId, CancellationToken.None);

            var me = room.CrossPlayer == CurrentPlayerId ?
                GameConstants.Cross :
                GameConstants.Nought;

            await Clients.Caller.RejoinGameRoom(me, room);
        }
    }

    public async Task JoinWaitingQueue()
    {
        var playerAdded = await waitingQueue.AddPlayer(CurrentPlayerId, CancellationToken.None);

        if (playerAdded)
        {
            await Clients.Caller.WaitingQueueJoined();
        }
    }

    public async Task ExitWaitingQueue()
    {
        var playerRemoved = await waitingQueue.RemovePlayer(CurrentPlayerId, CancellationToken.None);

        if (playerRemoved)
        {
            await Clients.Caller.WaitingQueueExited();
        }
    }

    public async Task PlayTurn(int row, int column)
    {
        var gameRoomId = await playerStore.GetGameRoomId(CurrentPlayerId, CancellationToken.None);
        ArgumentNullException.ThrowIfNull(gameRoomId);

        var gameRoom = await gameRoomStore.Get(gameRoomId, CancellationToken.None);

        if (gameRoom.GetCurrentPlayer() != CurrentPlayerId)
        {
            throw new InvalidOperationException("Player not on turn");
        }

        gameRoom.PlayTurn(row, column);

        //TODO: Check for game end

        var crossPlayerConnectionId = await playerStore.GetConnection(gameRoom.CrossPlayer, CancellationToken.None);
        var noughtPlayerConnectionId = await playerStore.GetConnection(gameRoom.NoughtPlayer, CancellationToken.None);

        ArgumentNullException.ThrowIfNull(crossPlayerConnectionId);
        ArgumentNullException.ThrowIfNull(noughtPlayerConnectionId);

        await Clients
            .Client(crossPlayerConnectionId.Value)
            .GameRoomStateUpdated(GameConstants.Cross, gameRoom);

        await Clients
            .Client(noughtPlayerConnectionId.Value)
            .GameRoomStateUpdated(GameConstants.Nought, gameRoom);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var existingConnection = await playerStore.GetConnection(CurrentPlayerId, CancellationToken.None);

        if (existingConnection == CurrentConnectionId)
        {
            await waitingQueue.RemovePlayer(CurrentPlayerId, CancellationToken.None);
            await playerStore.RemoveConnection(CurrentPlayerId, CancellationToken.None);
        }
    }
}
