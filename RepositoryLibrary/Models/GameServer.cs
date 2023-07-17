namespace RepositoryLibrary.Models;

public class GameServer
{
  public Guid Id;
  public int  Port;

  public List<Room>? Rooms;

  public DateTime CreatedAt = DateTime.Now;
}