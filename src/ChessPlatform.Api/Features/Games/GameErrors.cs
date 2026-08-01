using ChessPlatform.Api.Features.Common;

namespace ChessPlatform.Api.Features.Games;

public class GameErrors
{
    public static readonly Error InvalidPieceColor = new ("Game.InvalidPieceColor", "Invalid piece color chosen. Valid choices: White, Black");
    public static readonly Error InvalidTimeLimit = new ("Game.InvalidTimeLimit", "Invalid time limit");
    public static readonly Error InvalidMove = new ("Game.InvalidMove", "Invalid move");
    public static readonly Error GameNotFound = new ("Game.GameNotFound", "Game with given id not found");
    public static readonly Error NotPlayerInGame = new ("Game.NotPlayerInGame", "Player with given userId not found in game");
    public static readonly Error NotYourTurn = new ("Game.NotYourTurn", "It is not your turn to play");
}