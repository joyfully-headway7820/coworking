namespace coworking.Services;

public sealed class RuleViolation
{
    private RuleViolation(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }

    public int StatusCode { get; }

    public string Message { get; }

    public static RuleViolation NotFound(string message) => new(StatusCodes.Status404NotFound, message);

    public static RuleViolation Invalid(string message) => new(StatusCodes.Status400BadRequest, message);

    public static RuleViolation Conflict(string message) => new(StatusCodes.Status409Conflict, message);
}
