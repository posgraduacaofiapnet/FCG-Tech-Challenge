namespace FCG.Domain.Entities;

public class Promotion(string name, Guid gameId, decimal discountPercentage, DateTime startDate, DateTime endDate) : Entity
{
    public string Name { get; private set; } = name;
    public Guid GameId { get; private set; } = gameId;
    public Game Game { get; private set; } = null!;
    public decimal DiscountPercentage { get; private set; } = discountPercentage;
    public DateTime StartDate { get; private set; } = startDate;
    public DateTime EndDate { get; private set; } = endDate;
    public bool IsActive { get; private set; } = true;

    protected Promotion() : this(string.Empty, Guid.Empty, 0, DateTime.MinValue, DateTime.MinValue)
    {
    }

    public decimal CalculateDiscountAmount(decimal originalPrice)
    {
        if (!IsActive || DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
        {
            return 0;
        }

        return originalPrice * (DiscountPercentage / 100);
    }

    public void Update(string name, decimal discountPercentage, DateTime startDate, DateTime endDate)
    {
        Name = name;
        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
