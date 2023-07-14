namespace MasterServer.Managers;

public class PlayerPingManager
{
    private readonly Dictionary<Guid, DateTime> _playerPings;

    public PlayerPingManager()
    {
        _playerPings = new Dictionary<Guid, DateTime>();
    }

    public void PingPlayer(Guid id)
    {
        if (_playerPings.TryGetValue(id, out DateTime time))
        {
            _playerPings[id] = DateTime.Now;
        }
        else
        {
            _playerPings.Add(id, DateTime.Now);
        }
    }
    
    public void RemovePlayer(Guid id)
    {
        _playerPings.Remove(id);
    }
    
    public async Task<bool> IsPlayerActive(Guid id)
    {
        return await Task.Run(() =>
        {
            if (!_playerPings.TryGetValue(id, out DateTime time)) return false;
            if (DateTime.Now - time <= TimeSpan.FromSeconds(1)) return true;
                
            _playerPings.Remove(id);
            
            return false;
        });
    }
}