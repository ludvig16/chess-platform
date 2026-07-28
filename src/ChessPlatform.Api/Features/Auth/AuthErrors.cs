using ChessPlatform.Api.Features.Common;

namespace ChessPlatform.Api.Features.Auth;

public static class AuthErrors
{
    public static readonly Error UserNotFound = new("Auth.UserNotFound", "User with this username doesn't exist");
    public static readonly Error UsernameAlreadyInUse = new Error("Auth.UsernameAlreadyInUse", "Username is already in use");
    public static readonly Error EmailAlreadyInUse = new Error("Auth.EmailAlreadyInUse", "Email is already in use");
    public static readonly Error InvalidCredentials = new Error("Auth.InvalidCredentials", "Invalid credentials");
}