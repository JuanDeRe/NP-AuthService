namespace AuthService.Models;

public record LoginResponse(string Token, string UserId, string Username, string Role);