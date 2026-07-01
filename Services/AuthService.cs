using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        var user = _userRepository.GetByUsername(request.Username);

        if (user is null)
        {
            return null;
        }

        if (user.Password != request.Password)
        {
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(
            token,
            user.UserId,
            user.Username,
            user.Role
        );
    }
}