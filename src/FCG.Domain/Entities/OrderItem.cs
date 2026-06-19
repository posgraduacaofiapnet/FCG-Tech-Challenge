namespace FCG.Domain.Entities;

public class OrderItem(Guid gameId, decimal priceAtPurchase) : Entity
{
    public Guid OrderId { get; set; }
    public Guid GameId { get; set; } = gameId;
    public decimal PriceAtPurchase { get; set; } = priceAtPurchase;
    public Game Game { get; set; } = null!;

    protected OrderItem() : this(Guid.Empty, 0)
    {
    }
}
