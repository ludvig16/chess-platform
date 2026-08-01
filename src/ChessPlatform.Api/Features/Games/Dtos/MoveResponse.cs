namespace ChessPlatform.Api.Features.Games.Dtos;

public record MoveResponse(
    int Id,
    int GameId,
    string From,
    string To,
    int MoveNumber,
    string FenAfterMove,
    DateTime PlayedAt
);