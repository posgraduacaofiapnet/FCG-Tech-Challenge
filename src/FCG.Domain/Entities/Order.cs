using FCG.Domain.Common;
using FCG.Domain.Enums;

namespace FCG.Domain.Entities;

public class Order(Guid userId) : Entity
{
    public Guid UserId { get; set; } = userId;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public User User { get; set; } = null!;

    private readonly List<OrderItem> _items = [];

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    protected Order() : this(Guid.Empty)
    {
    }

    public Result AddItem(Guid gameId, decimal priceAtPurchase)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(Errors.Orders.CannotAddItemsToNonPendingOrder);
        }

        _items.Add(new OrderItem(gameId, priceAtPurchase));
        TotalAmount += priceAtPurchase;
        return Result.Success();
    }

    public Result MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(Errors.Orders.NotPending(Id));
        }

        Status = OrderStatus.Paid;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(Errors.Orders.OnlyPendingOrdersCanBeCanceled);
        }

        Status = OrderStatus.Canceled;
        return Result.Success();
    }
}
