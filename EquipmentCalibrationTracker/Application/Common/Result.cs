namespace Application.Common;

public enum ErrorType
{
    None,
    NotFound,
    Validation,
    Conflict
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorType ErrorType { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, ErrorType errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value)
        => new(true, value, ErrorType.None, null);

    public static Result<T> Failure(ErrorType errorType, string message)
        => new(false, default, errorType, message);
}
