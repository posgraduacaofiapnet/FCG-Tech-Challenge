namespace FCG.Domain.Entities;

public class LibraryItem(Guid userId, Guid gameId) : Entity
{
    public Guid UserId { get; private set; } = userId;
    public User User { get; private set; } = null!;

    public Guid GameId { get; private set; } = gameId;
    public Game Game { get; private set; } = null!;

    public DateTime AcquiredAt { get; private set; } = DateTime.UtcNow;

    protected LibraryItem() : this(Guid.Empty, Guid.Empty)
    {
    }
}
