namespace CardTrackerWebApi.Responses;

public class UserResponse
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required bool IsAdmin { get; init; }
}