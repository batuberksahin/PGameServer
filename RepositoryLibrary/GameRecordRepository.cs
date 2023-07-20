using RepositoryLibrary.Models;

namespace RepositoryLibrary;

public class GameRecordRepository : RepositoryBase<GameRecord>
{
  public GameRecordRepository(string collectionName) : base("GameRecords")
  {
  }

  protected override string GetId(GameRecord entity)
  {
    return entity.Id.ToString();
  }

  protected override Guid GetGuid(GameRecord entity)
  {
    return entity.Id;
  }
}