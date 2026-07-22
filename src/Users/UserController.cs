using ChessPlatform.Infrastructure.Persistence;
using ChessPlatform.Users.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChessPlatform.Users;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();

        var userDtos = users.Select(u => u.ToUserResponseDto());

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound("User not found with given id: " + id);
        }
            
        return Ok(user.ToUserResponseDto());
    }
    

    /*
    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequestDto userRequestDto)
    {
        var userModel = userRequestDto.ToUserFromCreateDto();

        _dbContext.Users.Add(userModel);
        _dbContext.SaveChanges();
        
        return CreatedAtAction(
            nameof(GetById), 
            new { Id = userModel.Id }, 
            userModel.ToUserResponseDto()
        );
    }
    */
}