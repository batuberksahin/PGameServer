namespace RepositoryLibrary.Models;

public class Player
{
    public Guid Id;
    
    public string? Username;
    public int Score;

    public Guid? ActiveRoom;
    
    public DateTime CreatedAt = DateTime.Now;
}