namespace ChessPlatform.Api.Domain.Entities;

public class Move
{
    public int Id { get; set; }
    
    public required int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public required string From { get; set; }
    public required string To { get; set; }
    public required int MoveNumber { get; set; }
    
    public required string FenAfterMove { get; set; }
    
    public DateTime PlayedAt { get; set; }
}