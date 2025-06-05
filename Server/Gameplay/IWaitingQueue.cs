using Shared;

namespace Server.Gameplay;

public interface IWaitingQueue
{
    Task<bool> AddPlayer(PlayerId playerId, CancellationToken cancellationToken);

    Task<bool> RemovePlayer(PlayerId playerId, CancellationToken cancellationToken);

    Task<PlayerId[]> GetAllPlayers(CancellationToken cancellationToken);
}
