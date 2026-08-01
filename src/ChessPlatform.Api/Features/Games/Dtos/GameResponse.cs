using ChessPlatform.Api.Domain.Entities;

namespace ChessPlatform.Api.Features.Games.Dtos;

public record GameResponse(
    int Id,
    int? WhitePlayerId,
    int? BlackPlayerId,
    GameStatus Status,
    string? CurrentFen,
    PieceColor SideToMove,
    int MoveCount,
    GameTermination? Termination,
    Winner? Winner,
    int TimeLimitMs,
    int WhiteTimeRemainingMs,
    int BlackTimeRemainingMs,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    MoveResponse[] Moves
);