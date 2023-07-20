using MongoDB.Driver;
using RepositoryLibrary.Models;

namespace RepositoryLibrary;

public class PlayerRepository : RepositoryBase<Player>
{
  public PlayerRepository(string collectionName) : base("Players")
  {
  }

  protected override string GetId(Player entity)
  {
    return entity.Id.ToString();
  }

  protected override Guid GetGuid(Player entity)
  {
    return entity.Id;
  }

  public async Task<int> GetScore(Guid playerId)
  {
    var player = await GetByGuidAsync(playerId);
    
    return player.Score;
  }

  protected async Task UpdateScore(Guid playerId, int score)
  {
    var player = await GetByGuidAsync(playerId);
    
    player.Score = score;
    
    await UpdateAsync(player);
  }
}