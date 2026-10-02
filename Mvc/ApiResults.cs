using coworking.Dtos;
using coworking.Services;
using Microsoft.AspNetCore.Mvc;

namespace coworking.Mvc;

public static class ApiResults
{
    public static ObjectResult FromViolations(IReadOnlyList<RuleViolation> violations)
    {
        var code = violations.Any(item => item.StatusCode == StatusCodes.Status404NotFound)
            ? StatusCodes.Status404NotFound
            : violations.Any(item => item.StatusCode == StatusCodes.Status409Conflict)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
        var message = violations.First(item => item.StatusCode == code).Message;

        return new ObjectResult(new ApiError
        {
            Message = message,
            Errors = violations.Select(item => item.Message).ToList()
        })
        {
            StatusCode = code
        };
    }
}
