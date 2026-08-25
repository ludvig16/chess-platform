using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChessPlatform.Api.Features.Games.Clock
{
    public class GameClockWorker : BackgroundService
    {
        private readonly ILogger<GameClockWorker> _logger;
        private readonly GameClockQueue _timeoutQueue;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly IHubContext<GameHub> _hubContext;

        public GameClockWorker(
            ILogger<GameClockWorker> logger, 
            GameClockQueue timeoutQueue, 
            IServiceScopeFactory scopeFactory,
            IHubContext<GameHub> hubContext
        )
        {
            _logger = logger;
            _timeoutQueue = timeoutQueue;
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        public async Task EndGameDueToTimeoutAsync(
            ChessDbContext db,
            Game game,
            GameClockEvent clockEvent,
            CancellationToken cancellationToken)
        {
            if (game.Status != GameStatus.InProgress)
                return;

            if (game.TurnExpiresAt is null)
                return;

            var difference = Math.Abs((clockEvent.ExpectedTurnExpiresAt!.Value - game.TurnExpiresAt.Value).TotalMilliseconds);

            if (difference > 100)
            {
                _logger.LogInformation(
                    "Ignoring stale clock event. GameId={GameId}, DifferenceMs={DifferenceMs}",
                    game.Id,
                    difference
                );

                return;
            }

            _logger.LogInformation(
                "Game timed out. GameId={GameId}, SideToMove={SideToMove}, TurnExpiresAt={TurnExpiresAt}",
                game.Id,
                game.SideToMove,
                game.TurnExpiresAt
            );

            game.Status = GameStatus.Finished;
            game.Termination = GameTermination.Timeout;
            game.FinishedAt = DateTime.UtcNow;

            game.Winner = game.SideToMove == PieceColor.White
                ? Winner.Black
                : Winner.White;

            if (game.SideToMove == PieceColor.White)
            {
                game.WhiteTimeRemainingMs = 0;
            }
            else
            {
                game.BlackTimeRemainingMs = 0;
            }

            game.TurnExpiresAt = null;

            await db.SaveChangesAsync(cancellationToken);

            await _hubContext
                .Clients
                .Group($"game:{game.Id}")
                .SendAsync(
                    "GameState",
                    game.ToGameResponseDto(),
                    cancellationToken
                );
        }

        private async Task ProcessEventAsync(GameClockEvent clockEvent, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChessDbContext>();

            var game = await db.Games
                .FirstOrDefaultAsync(g => g.Id == clockEvent.GameId, cancellationToken);

            if (game is null)
                return;

            switch (clockEvent.Type)
            {
                case GameClockEventType.TurnExpired:
                    await EndGameDueToTimeoutAsync(
                        db,
                        game,
                        clockEvent,
                        cancellationToken
                    );
                    break;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;

                    while (_timeoutQueue.TryDequeueDue(now, out var clockEvent))
                    {
                        _logger.LogInformation(
                            "Processing clock event. GameId={GameId}, Type={EventType}, ExecuteAt={ExecuteAt}",
                            clockEvent!.GameId,
                            clockEvent.Type,
                            clockEvent.ExecuteAt
                        );
                        await ProcessEventAsync(clockEvent!, stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing game clock");
                }
            }
        }
    }
}