using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Games.Dtos;

namespace ChessPlatform.Api.Features.Games;

public static class GameMappers
{
    public static GameResponse ToGameResponseDto(this Game game)
    {
        return new GameResponse(
            game.Id,
            game.WhitePlayerId,
            game.BlackPlayerId,
            game.Status,
            game.CurrentFen,
            game.SideToMove,
            game.MoveCount,
            game.Termination,
            game.Winner,
            game.TimeLimitMs,
            game.WhiteTimeRemainingMs,
            game.BlackTimeRemainingMs,
            game.CreatedAt,
            game.StartedAt,
            game.FinishedAt,
            game.Moves.Select(m => m.ToMoveResponseDto()).ToArray()
        );
    }

    public static MoveResponse ToMoveResponseDto(this Move move)
    {
        return new MoveResponse(
            move.Id,
            move.GameId,
            move.From,
            move.To,
            move.MoveNumber,
            move.FenAfterMove,
            move.PlayedAt
        );
    }
}