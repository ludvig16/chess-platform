namespace ChessPlatform.Api.Features.Games.Dtos;

public record CreateGameRequest(
    string ChosenColor,
    int TimeLimitMs
);