namespace RepositoryLibrary.Models;

public class GameRecord
{
  public Guid Id;
  public Guid GameServerId;
  public Guid RoomId;
  public List<Guid>? PlayerIdsOrderedByRank;
}