using Shared;

namespace Server.Gameplay;

public interface IPlayerStore
{
    Task<ConnectionId?> SetConnection(PlayerId playerId, ConnectionId connectionId,
        CancellationToken cancellationToken);

    Task RemoveConnection(PlayerId playerId, CancellationToken cancellationToken);

    Task<ConnectionId?> GetConnection(PlayerId playerId, CancellationToken cancellationToken);

    Task<GameRoomId?> GetGameRoomId(PlayerId playerId, CancellationToken cancellationToken);

    Task<KeyValuePair<PlayerId, ConnectionId>[]> GetAllConnections(CancellationToken cancellationToken);

    Task SetGameRoomId(PlayerId playerId, GameRoomId roomId, CancellationToken cancellationToken);

    Task<KeyValuePair<PlayerId, GameRoomId>[]> GetAllGameRooms(CancellationToken cancellationToken);
}
