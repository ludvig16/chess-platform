using ChessDotNet;
using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Common;
using ChessPlatform.Api.Features.Games.Clock;
using ChessPlatform.Api.Features.Games.Dtos;
using ChessPlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Move = ChessPlatform.Api.Domain.Entities.Move;

namespace ChessPlatform.Api.Features.Games;

public class GameService
{
    // 1 minute
    private const int MinAllowedTimelimitMs = 60000;
    
    // 10 minutes
    private const int MaxAllowedTimelimitMs = 600000;
    
    private readonly ChessDbContext _db;

    private readonly GameClockQueue _gameClockQueue;

    private readonly ILogger<GameService> _logger;

    private readonly IHubContext<GameHub> _hubContext;

    public GameService(
        ChessDbContext db, 
        GameClockQueue gameClockQueue, 
        ILogger<GameService> logger, 
        IHubContext<GameHub> hubContext
    )
    {
        _db = db;
        _gameClockQueue = gameClockQueue;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Result<Game>> GetGame(int gameId)
    {
        var game = await _db.Games
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        return game is null ? Result<Game>.Failure(GameErrors.GameNotFound) : Result<Game>.Success(game);
    }

    public async Task<Result<Game>> FetchJoinedGameAsync(int userId)
    {
        var game = await _db.Games
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => (g.WhitePlayerId == userId || g.BlackPlayerId == userId) && g.Status != GameStatus.Finished);

        if (game is not null)
        {
            UpdateCurrentPlayerTime(game);
            return Result<Game>.Success(game);
        }

        return Result<Game>.Failure(GameErrors.NoJoinedGame);
    }

    public async Task<Result<Piece[][]>> GetBoard(int gameId)
    {
        var game = await _db.Games.FindAsync(gameId);
        
        if (game is null) return Result<Piece[][]>.Failure(GameErrors.GameNotFound);
        
        var chess = new ChessGame(game.CurrentFen);

        return Result<Piece[][]>.Success(chess.GetBoard());
    }

    private async Task<bool> CheckIfUserHasAnOngoingGame(int userId)
    {
        var game = await _db.Games
            .FirstOrDefaultAsync(g => (g.WhitePlayerId == userId || g.BlackPlayerId == userId) && g.Status != GameStatus.Finished);

        return game is not null;
    }

    public async Task<Result<Game>> CreateGame(int userId, PieceColor chosenColor, int timeLimitMs)
    {
        var hasOngoingGame = await CheckIfUserHasAnOngoingGame(userId);
        if (hasOngoingGame) return Result<Game>.Failure(GameErrors.AlreadyInGame);

        if (!Enum.IsDefined(chosenColor))
        {
            return Result<Game>.Failure(GameErrors.InvalidPieceColor);
        }
        
        if (timeLimitMs is < MinAllowedTimelimitMs or > MaxAllowedTimelimitMs)
        {
            return Result<Game>.Failure(GameErrors.InvalidTimeLimit);
        }
        
        var newGame = new Game
        {
            WhitePlayerId = chosenColor == PieceColor.White ? userId : null,
            BlackPlayerId = chosenColor == PieceColor.Black ? userId : null,
            Status = GameStatus.Waiting,
            CurrentFen = new ChessGame().GetFen(),
            SideToMove = PieceColor.White,
            MoveCount = 0,
            Termination = null,
            Winner = null,
            TimeLimitMs = timeLimitMs,
            WhiteTimeRemainingMs = timeLimitMs,
            BlackTimeRemainingMs = timeLimitMs,
            CreatedAt = DateTime.UtcNow,
            StartedAt = null,
            FinishedAt = null
        };

        _db.Games.Add(newGame);
        await _db.SaveChangesAsync();

        return Result<Game>.Success(newGame);
    }

    public void StartGame(Game game)
    {
        game.Status = GameStatus.InProgress;

        var currentTime = DateTime.UtcNow;
        game.StartedAt = DateTime.UtcNow;

        game.TurnStartedAt = currentTime;
        game.TurnExpiresAt = currentTime.AddMilliseconds(game.WhiteTimeRemainingMs);

        _gameClockQueue.Enqueue(
            new GameClockEvent(
                game.Id,
                GameClockEventType.TurnExpired,
                game.TurnExpiresAt.Value,
                game.TurnExpiresAt
            )
        );
    }

    public async Task<Result<Game>> JoinGameAsync(int gameId, int userId)
    {
        var hasOngoingGame = await CheckIfUserHasAnOngoingGame(userId);
        if (hasOngoingGame) return Result<Game>.Failure(GameErrors.AlreadyInGame);

        var game = await _db.Games.FindAsync(gameId);
        
        if (game is null) return Result<Game>.Failure(GameErrors.GameNotFound);

        if (game.WhitePlayerId is not null && game.BlackPlayerId is not null)
        {
            return Result<Game>.Failure(GameErrors.GameIsFull);
        }

        if (game.WhitePlayerId is null)
        {
            game.WhitePlayerId = userId;
        }
        else
        {
            game.BlackPlayerId = userId;
        }

        StartGame(game);

        _db.Games.Update(game);
        await _db.SaveChangesAsync();

        await _hubContext
            .Clients
            .Group($"game:{game.Id}")
            .SendAsync(
                "GameState",
                game.ToGameResponseDto()
            );
        
        return Result<Game>.Success(game);
    }

    private static Result<Player> GetPlayer(Game game, int playerId)
    {
        if (game.WhitePlayerId == playerId) return Result<Player>.Success(Player.White);
        if (game.BlackPlayerId == playerId) return Result<Player>.Success(Player.Black);

        return Result<Player>.Failure(GameErrors.NotPlayerInGame);
    }

    private static GameTermination? CheckForGameEndCondition(ChessGame chess, Player opponent)
    {
        if (chess.IsCheckmated(opponent)) return GameTermination.Checkmate;
        if (chess.IsStalemated(opponent)) return GameTermination.Stalemate;
        if (chess.IsInsufficientMaterial()) return GameTermination.InsufficientMaterial;

        return null;
    }

    private static Result<bool> TryApplyMove(ChessGame chess, Player player, CreateMoveRequest request)
    {
        if (chess.WhoseTurn != player) return Result<bool>.Failure(GameErrors.NotYourTurn);

        var move = new ChessDotNet.Move(request.From, request.To, player);

        if (!chess.IsValidMove(move)) return Result<bool>.Failure(GameErrors.InvalidMove);

        chess.MakeMove(move, true);

        return Result<bool>.Success(true);
    }

    private static Move CreateMove(Game game, ChessGame chess, CreateMoveRequest request)
    {
        game.CurrentFen = chess.GetFen();
        game.MoveCount++;

        game.SideToMove = chess.WhoseTurn == Player.White ? PieceColor.White : PieceColor.Black;

        return new Move
        {
            GameId = game.Id,
            From = request.From,
            To = request.To,
            MoveNumber = game.MoveCount,
            FenAfterMove = game.CurrentFen
        };
    }

    public bool CheckIfGameTimeHasExpired(Game game)
    {
        var currentTime = DateTimeOffset.UtcNow;
        return game.Status == GameStatus.InProgress && game.TurnExpiresAt <= currentTime;
    }

    private static void UpdateCurrentPlayerTime(Game game)
    {
        var currentTime = DateTime.UtcNow;

        if (game.TurnStartedAt is null) return;

        var elapsedMs = (int)(currentTime - game.TurnStartedAt.Value).TotalMilliseconds;

        if (game.SideToMove == PieceColor.White)
        {
            game.WhiteTimeRemainingMs = Math.Max(
                0,
                game.WhiteTimeRemainingMs - elapsedMs
            );
        }
        else
        {
            game.BlackTimeRemainingMs = Math.Max(
                0,
                game.BlackTimeRemainingMs - elapsedMs
            );
        }
    }

    private void StartCurrentTurn(Game game)
    {
        var currentTime = DateTime.UtcNow;

        game.TurnStartedAt = currentTime;

        var remainingMs = game.SideToMove == PieceColor.White
            ? game.WhiteTimeRemainingMs
            : game.BlackTimeRemainingMs;

        game.TurnExpiresAt = currentTime.AddMilliseconds(remainingMs);

        _gameClockQueue.Enqueue(
            new GameClockEvent(
                game.Id,
                GameClockEventType.TurnExpired,
                game.TurnExpiresAt.Value,
                game.TurnExpiresAt
            )
        );
    }

    public async Task<Result<Game>> MakeMoveAsync(int playerId, int gameId, CreateMoveRequest request)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        
        var game = await _db.Games.FindAsync(gameId);

        if (game is null) return Result<Game>.Failure(GameErrors.GameNotFound);

        if (game.Status != GameStatus.InProgress) return Result<Game>.Failure(GameErrors.GameNotInProgress);

        _logger.LogInformation(
            "MakeMove starting. GameId={GameId}, PlayerId={PlayerId}, SideToMove={SideToMove}, WhiteTime={WhiteTime}ms, BlackTime={BlackTime}ms, TurnStartedAt={TurnStartedAt}, TurnExpiresAt={TurnExpiresAt}",
            game.Id,
            playerId,
            game.SideToMove,
            game.WhiteTimeRemainingMs,
            game.BlackTimeRemainingMs,
            game.TurnStartedAt,
            game.TurnExpiresAt
        );


        if (CheckIfGameTimeHasExpired(game))
        {
            EndGame(game, GameTermination.Timeout);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result<Game>.Success(game);
        }
        
        var chess = new ChessGame(game.CurrentFen);
        
        var player = GetPlayer(game, playerId);

        if (player.IsFailure) return Result<Game>.Failure(player.Error);

        var moveResult = TryApplyMove(chess, player.Value, request);

        if (moveResult.IsFailure) return Result<Game>.Failure(moveResult.Error);

        UpdateCurrentPlayerTime(game);
        
        var databaseMove = CreateMove(game, chess, request);
        
        _db.Moves.Add(databaseMove);

        _logger.LogInformation(
            "MakeMove completed. GameId={GameId}, PlayerId={PlayerId}, SideToMove={SideToMove}, WhiteTime={WhiteTime}ms, BlackTime={BlackTime}ms, TurnStartedAt={TurnStartedAt}, TurnExpiresAt={TurnExpiresAt}",
            game.Id,
            playerId,
            game.SideToMove,
            game.WhiteTimeRemainingMs,
            game.BlackTimeRemainingMs,
            game.TurnStartedAt,
            game.TurnExpiresAt
        );

        var opponent = player.Value == Player.White ? Player.Black : Player.White;

        var gameTermination = CheckForGameEndCondition(chess, opponent);

        if (gameTermination is not null)
        {
            EndGame(game, gameTermination);
        }
        else
        {
            StartCurrentTurn(game);
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<Game>.Success(game);
    }

    public async Task<Result<Game>> ResignAsync(int gameId, int playerId)
    {
        var game = await _db.Games.FindAsync(gameId);
        if (game is null) return Result<Game>.Failure(GameErrors.GameNotFound);
        
        var player = GetPlayer(game, playerId);
        if (player.IsFailure) return Result<Game>.Failure(player.Error);

        if (game.Status != GameStatus.InProgress) return Result<Game>.Failure(GameErrors.GameNotInProgress);

        game.Status = GameStatus.Finished;
        game.Termination = GameTermination.Resignation;
        
        game.Winner = game.SideToMove == PieceColor.White ? Winner.Black : Winner.White;
        
        _db.Games.Update(game);
        await _db.SaveChangesAsync();
        
        return Result<Game>.Success(game);
    }

    public Result<Game> EndGame(Game game, GameTermination? reason)
    {
        if (reason is null) return Result<Game>.Failure(GameErrors.InvalidAction);

        game.Status = GameStatus.Finished;
        game.Termination = reason;

        game.Winner = game.SideToMove == PieceColor.White ? Winner.Black : Winner.White;

        _logger.LogInformation(
            "Game terminated. GameId={GameId}, Reason={Reason}, SideToMove={SideToMove}, WhiteTime={WhiteTime}ms, BlackTime={BlackTime}ms, TurnStartedAt={TurnStartedAt}, TurnExpiresAt={TurnExpiresAt}",
            game.Id,
            reason,
            game.SideToMove,
            game.WhiteTimeRemainingMs,
            game.BlackTimeRemainingMs,
            game.TurnStartedAt,
            game.TurnExpiresAt
        );
        
        return Result<Game>.Success(game);
    }
}