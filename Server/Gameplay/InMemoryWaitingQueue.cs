using Shared;
using System.Collections.Concurrent;

namespace Server.Gameplay;

public sealed class InMemoryWaitingQueue : IWaitingQueue
{
    private readonly ConcurrentDictionary<PlayerId, bool> dictionary = [];

    public Task<bool> AddPlayer(PlayerId playerId, CancellationToken cancellationToken)
    {
        var added = dictionary.TryAdd(playerId, true);
        return Task.FromResult(added);
    }

    public Task<PlayerId[]> GetAllPlayers(CancellationToken cancellationToken)
    {
        return Task.FromResult(dictionary.Select(x => x.Key).ToArray());
    }

    public Task<bool> RemovePlayer(PlayerId playerId, CancellationToken cancellationToken)
    {
        var removed = dictionary.Remove(playerId, out _);
        return Task.FromResult(removed);
    }
}
