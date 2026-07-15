namespace AuthService.Models;

public class RegisterResponse
{
    public string PlayerId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}