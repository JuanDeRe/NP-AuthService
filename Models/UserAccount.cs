namespace AuthService.Models;

public class UserAccount
{
    public string Username { get; set; } =
        string.Empty;

    public string PlayerId { get; set; } =
        string.Empty;

    public string PasswordHash { get; set; } =
        string.Empty;

    public string Role { get; set; } =
        "Player";

    public DateTime CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }
}