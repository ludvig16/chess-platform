
namespace ChessPlatform.Api.Features.Games.Clock
{
    public class GameClockQueue
    {
        private readonly PriorityQueue<GameClockEvent, DateTime> _queue = new ();

        private readonly ILogger<GameClockQueue> _logger;

        private readonly object _lock = new ();

        public GameClockQueue(ILogger<GameClockQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(GameClockEvent clockEvent)
        {
            lock (_lock)
            {
                _queue.Enqueue(clockEvent, clockEvent.ExecuteAt);

                _logger.LogInformation(
                    "Clock event enqueued. GameId={GameId}, Type={EventType}, ExecuteAt={ExecuteAt}, ExpectedTurnExpiresAt={ExpectedTurnExpiresAt}",
                    clockEvent.GameId,
                    clockEvent.Type,
                    clockEvent.ExecuteAt,
                    clockEvent.ExpectedTurnExpiresAt
                );
            }
        }

        public bool TryDequeueDue(DateTime now, out GameClockEvent? clockEvent)
        {
            lock (_lock)
            {
                if (!_queue.TryPeek(out _, out var priority) || priority > now)
                {
                    clockEvent = null;
                    return false;
                }

                _queue.TryDequeue(out clockEvent, out _);

                _logger.LogInformation(
                    "Clock event dequeued. GameId={GameId}, Type={EventType}, ExecuteAt={ExecuteAt}, Now={Now}",
                    clockEvent!.GameId,
                    clockEvent.Type,
                    clockEvent.ExecuteAt,
                    now
                );

                return true;
            }
        }
    }
}