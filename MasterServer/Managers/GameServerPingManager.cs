using RepositoryLibrary.Models;

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
    if (_gameServerPings.TryGetValue(id, out var time))
      _gameServerPings[id] = DateTime.Now;
    else
      _gameServerPings.Add(id, DateTime.Now);
  }

  public void RemoveGameServer(Guid id)
  {
    _gameServerPings.Remove(id);
  }

  public Guid GetActiveGameServerGuid()
  {
    return _gameServerPings.FirstOrDefault(x => IsGameServerActive(x.Key).Result).Key;
  }

  private async Task<bool> IsGameServerActive(Guid id)
  {
    return await Task.Run(() =>
                          {
                            if (!_gameServerPings.TryGetValue(id, out var time)) return false;
                            if (DateTime.Now - time <= TimeSpan.FromSeconds(1)) return true;

                            _gameServerPings.Remove(id);

                            return false;
                          });
  }
}