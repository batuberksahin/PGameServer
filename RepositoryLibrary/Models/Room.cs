namespace RepositoryLibrary.Models;

public class Room
{
    public Guid Id;
    public List<Player>? ActivePlayers;
    public int Capacity;
    
    public DateTime CreatedAt = DateTime.Now;
}