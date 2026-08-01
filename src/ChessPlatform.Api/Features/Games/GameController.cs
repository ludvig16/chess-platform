using System.Security.Claims;
using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Games.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessPlatform.Api.Features.Games;

[Authorize]
[ApiController]
[Route("api/games")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    
    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var result = await _gameService.GetGame(id);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value!.ToGameResponseDto());
    }

    [HttpGet("{id}/board")]
    public async Task<IActionResult> GetBoard([FromRoute] int id)
    {
        var result = await _gameService.GetBoard(id);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateGame(CreateGameRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _gameService.CreateGame(
            int.Parse(userId), 
            Enum.Parse<PieceColor>(request.ChosenColor), 
            request.TimeLimitMs
        );

        if (result.IsFailure)
        {
            if (result.Error == GameErrors.GameNotFound)
            {
                return NotFound(result.Error);
            }
                
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value!.ToGameResponseDto());
    }

    [HttpPost("{id}/moves")]
    public async Task<IActionResult> CreateMove(CreateMoveRequest request, [FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _gameService.MakeMove(int.Parse(userId), id, request);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToMoveResponseDto());
    }
}