namespace FCG.Domain.Entities;

public class Game(string title, string description, decimal price) : Entity
{
    public string Title { get; private set; } = title;
    public string Description { get; private set; } = description;
    public decimal Price { get; private set; } = price;
    public bool IsActive { get; private set; } = true;

    public ICollection<LibraryItem> LibraryItems { get; private set; } = [];
    public ICollection<Promotion> Promotions { get; private set; } = [];

    protected Game() : this(string.Empty, string.Empty, 0)
    {
    }

    public void Update(string title, string description, decimal price)
    {
        Title = title;
        Description = description;
        Price = price;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
