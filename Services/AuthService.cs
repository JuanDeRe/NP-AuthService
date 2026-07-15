using AuthService.Exceptions;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

/// <summary>
/// Registers and authenticates users stored in DynamoDB.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);

        var normalizedUsername = NormalizeUsername(
            request.Username
        );

        var user = new UserAccount
        {
            Username = normalizedUsername,
            PlayerId = $"player-{Guid.NewGuid():N}",
            Role = "Player",
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password
            );

        var created = await _userRepository.CreateAsync(
            user,
            cancellationToken
        );

        if (!created)
        {
            throw new UsernameAlreadyExistsException(
                request.Username.Trim()
            );
        }

        return new RegisterResponse
        {
            PlayerId = user.PlayerId,
            Username = user.Username,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var normalizedUsername = NormalizeUsername(
            request.Username
        );

        var user = await _userRepository
            .GetByUsernameAsync(
                normalizedUsername,
                cancellationToken
            );

        if (user is null || user.IsActive != true)
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(
            token,
            user.PlayerId,
            user.Username,
            user.Role
        );
    }

    private static void ValidateRegistration(
        RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException(
                "Username is required."
            );
        }

        var username = request.Username.Trim();

        if (username.Length < 3)
        {
            throw new ArgumentException(
                "Username must contain at least 3 characters."
            );
        }

        if (username.Length > 24)
        {
            throw new ArgumentException(
                "Username cannot contain more than 24 characters."
            );
        }

        if (!username.All(character =>
                char.IsLetterOrDigit(character) ||
                character == '_' ||
                character == '-'))
        {
            throw new ArgumentException(
                "Username can only contain letters, numbers, hyphens and underscores."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Password is required."
            );
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters."
            );
        }

        if (request.Password.Length > 72)
        {
            throw new ArgumentException(
                "Password cannot contain more than 72 characters."
            );
        }
    }

    private static string NormalizeUsername(
        string username)
    {
        return username
            .Trim()
            .ToUpperInvariant();
    }
}