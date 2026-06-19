using FCG.Application.DTOs;
using FCG.Domain.Common;

namespace FCG.Application.Interfaces;

public interface IPromotionService
{
    Task<Result<PagedResult<PromotionDto>>> GetAllActiveAsync(int page);
    Task<Result<PromotionDto>> CreateAsync(CreatePromotionDto dto);
    Task<Result> DeactivateAsync(Guid id);
}
