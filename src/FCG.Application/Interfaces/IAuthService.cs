using FCG.Application.DTOs;
using FCG.Domain.Common;

namespace FCG.Application.Interfaces;

public interface IAuthService
{
    Task<Result<TokenDto>> LoginAsync(LoginDto dto);
    Task<Result> RegisterAsync(RegisterUserDto dto);
}
