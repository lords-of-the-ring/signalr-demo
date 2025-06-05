using Shared;
using System.Collections.Concurrent;

namespace Server.Gameplay;

public sealed class InMemoryPlayerStore : IPlayerStore
{
    private readonly ConcurrentDictionary<PlayerId, ConnectionId> connections = [];

    private readonly ConcurrentDictionary<PlayerId, GameRoomId> rooms = [];

    private static readonly Lock _lock = new();

    public Task<GameRoomId?> GetGameRoomId(PlayerId playerId, CancellationToken cancellationToken)
    {
        var room = rooms.GetValueOrDefault(playerId);
        return Task.FromResult(room);
    }

    public Task RemoveConnection(PlayerId playerId, CancellationToken cancellationToken)
    {
        connections.Remove(playerId, out _);
        return Task.CompletedTask;
    }

    public Task<ConnectionId?> SetConnection(PlayerId playerId, ConnectionId connectionId, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var oldConnectionId = connections.GetValueOrDefault(playerId);
            connections.AddOrUpdate(playerId, connectionId, (key, value) => connectionId);
            return Task.FromResult(oldConnectionId);
        }
    }

    public Task<ConnectionId?> GetConnection(PlayerId playerId, CancellationToken cancellationToken)
    {
        var connection = connections.GetValueOrDefault(playerId);
        return Task.FromResult(connection);
    }

    public Task<KeyValuePair<PlayerId, ConnectionId>[]> GetAllConnections(CancellationToken cancellationToken)
    {
        var result = connections.ToArray();
        return Task.FromResult(result);
    }

    public Task SetGameRoomId(PlayerId playerId, GameRoomId roomId, CancellationToken cancellationToken)
    {
        _ = rooms.TryAdd(playerId, roomId);
        return Task.CompletedTask;
    }

    public Task<KeyValuePair<PlayerId, GameRoomId>[]> GetAllGameRooms(CancellationToken cancellationToken)
    {
        var result = rooms.ToArray();
        return Task.FromResult(result);
    }
}
