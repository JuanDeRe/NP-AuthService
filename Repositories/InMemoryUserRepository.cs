using AuthService.Models;

namespace AuthService.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<AuthenticatedUser> _users =
    [
        new AuthenticatedUser
        {
            UserId = "player-001",
            Username = "juan",
            Password = "1234",
            Role = "player"
        }, 
        new AuthenticatedUser
        {
            UserId = "player-002",
            Username = "pepe",
            Password = "1234",
            Role = "player"
        },
        new AuthenticatedUser
        {
            UserId = "player-003",
            Username = "mark",
            Password = "1234",
            Role = "player"
        },
        new AuthenticatedUser
        {
            UserId = "admin-001",
            Username = "admin",
            Password = "admin123",
            Role = "admin"
        }
    ];

    public AuthenticatedUser? GetByUsername(string username)
    {
        return _users.FirstOrDefault(user =>
            user.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
        );
    }
}