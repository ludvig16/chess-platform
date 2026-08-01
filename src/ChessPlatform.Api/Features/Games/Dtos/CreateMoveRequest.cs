using System.ComponentModel.DataAnnotations;

namespace ChessPlatform.Api.Features.Games.Dtos;

public record CreateMoveRequest(
    [RegularExpression("^[A-H][1-8]$", ErrorMessage = "Invalid chess position.")]
    string From,
    [RegularExpression("^[A-H][1-8]$", ErrorMessage = "Invalid chess position.")]
    string To
);