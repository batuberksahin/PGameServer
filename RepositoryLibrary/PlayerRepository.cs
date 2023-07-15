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
}