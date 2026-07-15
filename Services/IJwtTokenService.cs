using AuthService.Models;

namespace AuthService.Services;

public interface IJwtTokenService
{
    string GenerateToken(UserAccount user);
}