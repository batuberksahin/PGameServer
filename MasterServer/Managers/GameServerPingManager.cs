namespace MasterServer.Managers;

public class GameServerPingManager
{
    private readonly Dictionary<Guid, DateTime> _gameServerPings;

    public GameServerPingManager()
    {
        _gameServerPings = new Dictionary<Guid, DateTime>();
    }

    public void PingGameServer(Guid id)
    {
        if (_gameServerPings.TryGetValue(id, out DateTime time))
        {
            _gameServerPings[id] = DateTime.Now;
        }
        else
        {
            _gameServerPings.Add(id, DateTime.Now);
        }
    }
    
    public void RemoveGameServer(Guid id)
    {
        _gameServerPings.Remove(id);
    }
    
    public async Task<bool> IsGameServerActive(Guid id)
    {
        return await Task.Run(() =>
        {
            if (!_gameServerPings.TryGetValue(id, out DateTime time)) return false;
            if (DateTime.Now - time <= TimeSpan.FromSeconds(1)) return true;
                
            _gameServerPings.Remove(id);
            
            return false;
        });
    }

}