using Shared;

namespace Server.Gameplay;

public interface IGameHubClient
{
    Task ForceDisconnect();

    Task GameRoomStateUpdated(char me, GameRoomState room);

    Task WaitingQueueJoined();

    Task WaitingQueueExited();

    Task GameRoomJoined(char me, GameRoomState room);

    Task RejoinGameRoom(char me, GameRoomState room);
}
