namespace AuthService.Exceptions;

public class UsernameAlreadyExistsException : Exception
{
    public UsernameAlreadyExistsException(string username)
        : base($"Username '{username}' is already registered.")
    {
    }
}