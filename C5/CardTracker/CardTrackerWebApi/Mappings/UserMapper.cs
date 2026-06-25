using CardTrackerWebApi.Models;
using CardTrackerWebApi.Responses;
using Riok.Mapperly.Abstractions;

namespace CardTrackerWebApi.Mappings;
[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    [MapperIgnoreSource(nameof(User.Salt))]
    public partial UserResponse ToResponse(User user);
}