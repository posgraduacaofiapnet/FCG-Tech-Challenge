namespace FCG.Application.DTOs;

public record RegisterUserDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record TokenDto(string Token);
