namespace ChessPlatform.Features.Common;

public class Result<T>
{
    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

public sealed record Error(string Code, string Description, string? StrackTrace = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Unexpected/System Failure (Capture StackTrace for debugging)
    public static Error Unexpected(string code, string description) => 
        new(code, description, Environment.StackTrace);
}