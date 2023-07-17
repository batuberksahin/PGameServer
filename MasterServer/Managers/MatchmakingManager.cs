using System.Net.Sockets;
using RepositoryLibrary.Models;

namespace MasterServer.Managers;

public class MatchmakingManager
{
  private Dictionary<Player, TcpClient>     _playersQueue;
  private Dictionary<GameServer, TcpClient> _gameServers;

  public MatchmakingManager()
  {
    _playersQueue = new Dictionary<Player, TcpClient>();
    _gameServers  = new Dictionary<GameServer, TcpClient>();
  }

  public void AddPlayer(Player player, TcpClient client)
  {
    _playersQueue.Add(player, client);
  }

  public void RemovePlayer(Player player)
  {
    _playersQueue.Remove(player);
  }

  public void AddGameServer(GameServer gameServer, TcpClient client)
  {
    _gameServers.Add(gameServer, client);
  }

  public void RemoveGameServer(GameServer gameServer)
  {
    _gameServers.Remove(gameServer);
  }

  public int GetPlayersCountInQueue()
  {
    return _playersQueue.Count;
  }

  public Dictionary<Player, TcpClient> GetPlayersFromQueue(int playerCount)
  {
    return _playersQueue.Take(playerCount).ToDictionary(x => x.Key, x => x.Value);
  }

  public GameServer GetGameServerByGuid(Guid id)
  {
    return _gameServers.FirstOrDefault(x => x.Key.Id == id).Key;
  }

  public TcpClient GetGameServerTcpClient(Guid id)
  {
    return _gameServers.FirstOrDefault(x => x.Key.Id == id).Value;
  }
}