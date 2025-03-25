
namespace Bartender.Domain;
public enum ErrorType
{
    NotFound,
    Unauthorized,
    Conflict,
    Validation,
    Unknown
}

public record ServiceResult<T>(
    bool Success,
    T? Data = default,
    string? Error = null,
    ErrorType? errorType = null)
{
    public static ServiceResult<T> Ok(T data) => new(true, data);
    public static ServiceResult<T> Fail(string error, ErrorType errorType = ErrorType.Unknown)
        => new(false, default, error, errorType);
}

public record ServiceResult(
    bool Success,
    string? Error = null,
    ErrorType? errorType = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string error, ErrorType errorType = ErrorType.Unknown)
        => new(false, error, errorType);
}
