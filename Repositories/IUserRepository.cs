using AuthService.Models;

namespace AuthService.Repositories;

public interface IUserRepository
{
    AuthenticatedUser? GetByUsername(string username);
}