using FCG.Domain.Enums;

namespace FCG.Domain.Entities;

public class User(string name, string email, string passwordHash, Role role) : Entity
{
    public string Name { get; private set; } = name;
    public string Email { get; private set; } = email;
    public string PasswordHash { get; private set; } = passwordHash;
    public Role Role { get; private set; } = role;
    public bool IsActive { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }
    public DateTime? ReactivatedAt { get; private set; }

    public ICollection<LibraryItem> LibraryItems { get; private set; } = new List<LibraryItem>();

    protected User() : this(string.Empty, string.Empty, string.Empty, Role.User)
    {
    }

    public void AddGameToLibrary(Game game)
    {
        if (!LibraryItems.Any(libraryItem => libraryItem.GameId == game.Id))
        {
            LibraryItems.Add(new LibraryItem(Id, game.Id));
        }
    }

    public void ChangeRole(Role role)
    {
        Role = role;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        ReactivatedAt = DateTime.UtcNow;
    }
}
