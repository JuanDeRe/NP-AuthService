using AuthService.Models;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetByUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken = default
    );

    Task<bool> CreateAsync(
        UserAccount user,
        CancellationToken cancellationToken = default
    );
}