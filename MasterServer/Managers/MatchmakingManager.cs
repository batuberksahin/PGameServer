using System.Net.Sockets;
using RepositoryLibrary.Models;

namespace MasterServer.Managers;

public class MatchmakingManager
{
    private Dictionary<Player, TcpClient> _players;

    public MatchmakingManager()
    {
        _players = new Dictionary<Player, TcpClient>();
    }
    
    public void AddPlayer(Player player, TcpClient client)
    {
        _players.Add(player, client);
    }

    public void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }
}