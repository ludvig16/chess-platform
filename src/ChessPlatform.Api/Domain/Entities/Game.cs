namespace ChessPlatform.Api.Domain.Entities;

public class Game
{
    public int Id { get; set; }
    
    public int? WhitePlayerId { get; set; }
    public int? BlackPlayerId { get; set; }
    public GameStatus Status { get; set; }
    public string? CurrentFen { get; set; }
    public PieceColor SideToMove { get; set; }
    public required int MoveCount { get; set; }
    
    public GameTermination? Termination { get; set; }
    public Winner? Winner { get; set; }
    
    public int TimeLimitMs { get; set; }
    public int WhiteTimeRemainingMs { get; set; }
    public int BlackTimeRemainingMs { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public ICollection<Move> Moves { get; set; } = new List<Move>();
}

public enum PieceColor
{
    White = 0,
    Black = 1
}

public enum GameTermination
{
    Checkmate = 0,
    Stalemate = 1,
    Resignation = 2,
    Timeout = 3,
    DrawAgreement = 4,
    ThreefoldRepetition = 5,
    FiftyMoveRule = 6,
    InsufficientMaterial = 7,
    Aborted = 8
}

public enum GameStatus
{
    Waiting = 0,
    InProgress = 1,
    Finished = 2
}

public enum Winner
{
    White = 0,
    Black = 1,
    Draw = 2,
}