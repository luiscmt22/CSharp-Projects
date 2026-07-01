using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CardTrackerWebApi.Endpoints;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users/{username}", HandleGetUserByUsername)
            .WithName("GetUserByUsername")
            .WithDescription("Gets a specific user by their username")
            .RequireAuthorization("AdminOnly")
            .Produces<UserResponse>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/users", HandleGetAllUsers)
            .WithName("GetAllUsers")
            .WithDescription("Gets all users")
            .RequireAuthorization("AdminOnly")
            .Produces<List<UserResponse>>();

        app.MapPost("/users", HandleAddUser)
            .WithName("AddUser")
            .WithDescription("Adds a new user to the system")
            .AllowAnonymous()
            .Produces<UserResponse>(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> HandleGetUserByUsername(string username, CardsDbContext db, UserMapper mapper)
    {
        User? user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.Equals(username));

        if (user is null)
            return Results.NotFound();

        UserResponse response = mapper.ToResponse(user);
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleGetAllUsers(CardsDbContext db, UserMapper mapper)
    {
        List<User> users = await db.Users.AsNoTracking().ToListAsync();

        List<UserResponse> response = [.. users.Select(mapper.ToResponse)];
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleAddUser(CreateUserRequest request, CardsDbContext db, UserMapper mapper)
    {
        request.Username = request.Username.ToLower();
        if (db.Users.AsNoTracking().Any(u => u.Username.Equals(request.Username))) 
            return Results.BadRequest($"A user already exists with a username of {request.Username}");

        byte[] salt = hasher.GenerateSalt();
        byte[] hash = hasher.ComputeHash(request.Password, salt);

        User user = new User { Username = request.Username, Salt = salt, PasswordHash = hash };

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        UserResponse response = mapper.ToResponse(user);
        return Results.Created($"/users/{request.Username}",response);
    }
}