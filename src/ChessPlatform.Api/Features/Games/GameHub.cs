using ChessPlatform.Api.Features.Games.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChessPlatform.Api.Features.Games;

[Authorize]
public class GameHub : Hub
{
    private readonly GameService _gameService;

    public GameHub(GameService gameService)
    {
        _gameService = gameService;
    }
    
    private int GetUserId()
    {
        return int.Parse(Context.UserIdentifier!);
    }

    private static string GetGroupName(int gameId)
    {
        return $"game:{gameId}";
    }

    public async Task JoinGame(int gameId)
    {
        Console.WriteLine("trying to join game");
        var userId = GetUserId();

        var result = await _gameService.JoinGameAsync(gameId, userId);

        if (result.IsFailure)
        {
            await Clients.Caller.SendAsync("ReceiveError", result.Error);
            return;
        }
        
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(gameId)
        );

        await Clients.Caller.SendAsync("JoinedGame", result.Value);
    }

    public async Task FetchJoinedGame()
    {
        var userId = GetUserId();

        var result = await _gameService.FetchJoinedGameAsync(userId);

        if (result.IsFailure)
        {
            await Clients.Caller.SendAsync("ReceiveError", result.Error);
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(result.Value!.Id)
        );

        await Clients.Caller.SendAsync("GameState", result.Value!.ToGameResponseDto());
    }
    
    public async Task MakeMove(int gameId, string from, string to)
    {
        var userId = GetUserId();
        
        CreateMoveRequest move = new (from, to);

        var result = await _gameService.MakeMoveAsync(userId, gameId, move);
        
        if (result.IsFailure)
        {
            await Clients.Caller.SendAsync("ReceiveError", result.Error);
            return;
        }

        await Clients.Group(
            GetGroupName(gameId))
            .SendAsync("GameState", result.Value!.ToGameResponseDto()
        );
    }

    public async Task Resign(int gameId)
    {
        var userId = GetUserId();
        
        var result = await _gameService.ResignAsync(gameId, userId);
        
        if (result.IsFailure)
        {
            await Clients.Caller.SendAsync("ReceiveError", result.Error);
            return;
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnected: {Context.ConnectionId}");

        await base.OnDisconnectedAsync(exception);
    }
}