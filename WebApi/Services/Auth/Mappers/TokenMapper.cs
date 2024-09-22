using WebApi.Data.Entities;
using WebApi.Services.Auth.Models;

namespace WebApi.Services.Auth.Mappers;

public static class TokenMapper
{
    public static User? ToUser(this TokenRequest? tokenRequest)
    {
        if (tokenRequest == null)
        {
            return null;
        }
        return new User
        {
            AvatarUrl = tokenRequest.AvatarUrl,
            Email = tokenRequest.Email,
            FullName = tokenRequest.FullName,
            Id = tokenRequest.Id,
            Role = tokenRequest.Role,
            Status = tokenRequest.Status,
        };
    }
}
