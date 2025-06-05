using Shared;

namespace Server.Gameplay;

public interface IGameRoomStore
{
    Task Add(GameRoomState room, CancellationToken cancellationToken);

    Task<GameRoomState> Get(GameRoomId id, CancellationToken cancellationToken);
}
