using Microsoft.EntityFrameworkCore;

namespace CardTrackerWebApi.Models;

[Index(nameof(Username), IsUnique = true)]
public class User : BaseEntity
{
    public required string Username { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] Salt { get; set; }
    public bool IsAdmin { get; set; }
}