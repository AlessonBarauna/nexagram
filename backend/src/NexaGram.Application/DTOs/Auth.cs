namespace NexaGram.Application.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? DisplayName);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    AuthUserDto User);

public record AuthUserDto(
    Guid Id,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    bool IsVerified);
