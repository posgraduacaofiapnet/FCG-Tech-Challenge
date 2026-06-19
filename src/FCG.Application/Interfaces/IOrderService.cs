using FCG.Application.DTOs;
using FCG.Domain.Common;

namespace FCG.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<Result<OrderDto>> GetOrderByIdAsync(Guid orderId, Guid currentUserId, bool isAdmin);
    Task<Result<IEnumerable<OrderDto>>> GetUserOrdersAsync(Guid userId);
    Task<Result> ApprovePaymentAsync(Guid orderId, Guid currentUserId, bool isAdmin);
}
