namespace ConferenceWebApp.Application;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);
    public static Result Failureure(string errorMessage) => new(false, errorMessage);
}

public class Result<T> : Result
{
    public T Value { get; }
    private Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null);
    public static Result<T> Failure(string errorMessage) => new(default, false, errorMessage);
}
