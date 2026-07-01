using AuthService.Models;

namespace AuthService.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}