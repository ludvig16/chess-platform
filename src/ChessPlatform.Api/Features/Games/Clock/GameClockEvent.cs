
namespace ChessPlatform.Api.Features.Games.Clock
{

    public enum GameClockEventType
    {
        StartGame = 0,
        TurnExpired = 1
    }

    public sealed record GameClockEvent
    (
        int GameId,
        GameClockEventType Type,
        DateTime ExecuteAt,
        DateTime? ExpectedTurnExpiresAt = null
    );
}