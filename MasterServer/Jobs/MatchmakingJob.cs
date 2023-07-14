using System.Net.Sockets;
using System.Text;
using MasterServer.Managers;
using NetworkLibrary;
using NetworkLibrary.Jobs;
using RepositoryLibrary.Models;

namespace MasterServer.Jobs;

public class MatchmakingJob : JobBase
{
    private const int MaxPlayers = 2;
    
    public override async Task RunAsync()
    {
        var playersCount = ManagerLocator.MatchmakingManager.GetPlayersCountInQueue();
        
        if (playersCount < MaxPlayers) return;
        
        var players = ManagerLocator.MatchmakingManager.GetPlayersFromQueue(MaxPlayers);
        var availableGameServerGuid = ManagerLocator.GameServerPingManager.GetActiveGameServerGuid();
        
        if (availableGameServerGuid == Guid.Empty)
            return;

        // TODO: remove players from queue after room created
        var gameServerTcpClient = ManagerLocator.MatchmakingManager.GetGameServerTcpClient(availableGameServerGuid);
        
        await Messenger.SendResponseAsync(gameServerTcpClient, "server_new_room", new
        {
            Players = players.Keys.ToList()
        });

        var roomId = await HandleResponseAsync(gameServerTcpClient);

        var gameServerInfo = ManagerLocator.MatchmakingManager.GetGameServerByGuid(availableGameServerGuid);
        
        foreach (KeyValuePair<Player,TcpClient> playerContainer in players)
        {
            await Messenger.SendResponseAsync(playerContainer.Value, "room_info", new
            {
                GameServerAddress = "127.0.0.1",
                GameServerPort = gameServerInfo.Port,
                RoomId = 
            });
        }
        
        Console.WriteLine($"[#] Sent game server address to {players.Count} players");
    }
    
    private async Task<Guid> HandleResponseAsync(TcpClient tcpClient)
    {
        var response = await ReadResponseAsync(tcpClient);
        // TODO: handle room id from json
        //JsonConvert.DeserializeObject<TRequest>(payload, _jsonSerializerSettings);
        /*string[] behaviourParts = behaviourString.Split(new[] { ':' }, 2);
        if (behaviourParts.Length < 1)
            return;

        string behaviourName = behaviourParts[0];
        string payload = behaviourParts[1];*/

    }

    private async Task<string> ReadResponseAsync(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        
        return response;
    }
}