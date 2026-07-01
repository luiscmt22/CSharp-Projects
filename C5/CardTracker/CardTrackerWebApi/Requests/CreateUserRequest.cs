namespace CardTrackerWebApi.Requests;

public class CreateUserRequest
{
    public required string Username { get; set; }
    public required byte[] Password { get; set; }
}