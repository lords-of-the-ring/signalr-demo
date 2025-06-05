using Shared;
using System.Collections.Concurrent;

namespace Server.Gameplay;

public sealed class InMemoryGameRoomStore : IGameRoomStore
{
    private readonly ConcurrentDictionary<GameRoomId, GameRoomState> rooms = [];

    public Task Add(GameRoomState room, CancellationToken cancellationToken)
    {
        _ = rooms.TryAdd(room.Id, room);
        return Task.CompletedTask;
    }

    public Task<GameRoomState> Get(GameRoomId id, CancellationToken cancellationToken)
    {
        var room = rooms.GetValueOrDefault(id);
        ArgumentNullException.ThrowIfNull(room);
        return Task.FromResult(room);
    }
}
