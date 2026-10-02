namespace coworking.Dtos;

public class ApiError
{
    public string Message { get; set; } = "";

    public List<string> Errors { get; set; } = [];
}
