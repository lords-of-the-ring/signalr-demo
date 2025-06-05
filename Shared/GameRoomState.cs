namespace Shared;

public sealed record GameRoomState
{
    public required GameRoomId Id { get; init; }

    public required PlayerId CrossPlayer { get; init; }

    public required PlayerId NoughtPlayer { get; init; }

    public List<List<char>> Board { get; init; } =
    [
        [GameConstants.EmptyCell, GameConstants.EmptyCell, GameConstants.EmptyCell],
        [GameConstants.EmptyCell, GameConstants.EmptyCell, GameConstants.EmptyCell],
        [GameConstants.EmptyCell, GameConstants.EmptyCell, GameConstants.EmptyCell],
    ];

    public char CurrentSymbol { get; set; } = GameConstants.Cross;

    public PlayerId GetCurrentPlayer()
    {
        return CurrentSymbol == GameConstants.Cross ? CrossPlayer : NoughtPlayer;
    }

    public void PlayTurn(int row, int column)
    {
        if (Board[row][column] != GameConstants.EmptyCell)
        {
            throw new InvalidOperationException("Cell already taken");
        }

        Board[row][column] = CurrentSymbol;
        CurrentSymbol = CurrentSymbol == GameConstants.Cross ? GameConstants.Nought : GameConstants.Cross;
    }

    public char GetPlayerSymbol(PlayerId playerId)
    {
        if (playerId == CrossPlayer)
        {
            return GameConstants.Cross;
        }

        return GameConstants.Nought;
    }
}
