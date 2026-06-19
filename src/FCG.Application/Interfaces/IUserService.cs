using FCG.Application.DTOs;
using FCG.Domain.Common;

namespace FCG.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto);
    Task<Result<PagedResult<UserDto>>> GetAllAsync(int page);
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<UserDto>> UpdateRoleAsync(Guid id, UpdateUserRoleDto dto);
    Task<Result<UserDto>> DeactivateAsync(Guid id);
    Task<Result<UserDto>> ReactivateAsync(Guid id);
}
