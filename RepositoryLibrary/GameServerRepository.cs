using RepositoryLibrary.Models;

namespace RepositoryLibrary;

public class GameServerRepository : RepositoryBase<GameServer>
{
    public GameServerRepository(string collectionName) : base("GameServers")
    {
    }

    protected override string GetId(GameServer entity)
    {
        return entity.Id.ToString();
    }

    protected override Guid GetGuid(GameServer entity)
    {
        return entity.Id;
    }
}