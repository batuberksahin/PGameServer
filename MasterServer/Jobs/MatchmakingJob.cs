using System.Net;
using System.Net.Sockets;
using System.Text;
using MasterServer.Managers;
using NetworkLibrary;
using NetworkLibrary.Jobs;
using Newtonsoft.Json;
using RepositoryLibrary.Models;

namespace MasterServer.Jobs;

public class MatchmakingJob : JobBase
{
  private const int MaxPlayers = 2;

  public override Task StartAsync()
  {
    return Task.CompletedTask;
  }
  
  public override async Task RunAsync()
  {
    try
    {
      var playersCount = ManagerLocator.MatchmakingManager.GetPlayersCountInQueue();

      if (playersCount < MaxPlayers) return;

      var players                 = ManagerLocator.MatchmakingManager.GetPlayersFromQueue(MaxPlayers);
      var availableGameServerGuid = ManagerLocator.GameServerPingManager.GetActiveGameServerGuid();

      if (availableGameServerGuid == Guid.Empty)
        return;

      var gameServer = ManagerLocator.MatchmakingManager.GetGameServerByGuid(availableGameServerGuid);

      var gameServerTcpClient = new TcpClient();
      await gameServerTcpClient.ConnectAsync(IPAddress.Parse("127.0.0.1"), gameServer.Port);

      //var gameServerTcpClient = ManagerLocator.MatchmakingManager.GetGameServerTcpClient(availableGameServerGuid);
      await Messenger.SendResponseAsync(gameServerTcpClient, "server_new_room", new
                                                                                {
                                                                                  Players = players.Keys.ToList()
                                                                                });

      var roomId = await HandleResponseAsync(gameServerTcpClient);

      var gameServerInfo = ManagerLocator.MatchmakingManager.GetGameServerByGuid(availableGameServerGuid);

      foreach (KeyValuePair<Player, TcpClient> playerContainer in players)
      {
        await Messenger.SendResponseAsync(playerContainer.Value, "room_info", new
                                                                              {
                                                                                GameServerAddress = "127.0.0.1",
                                                                                GameServerPort    = gameServerInfo.Port,
                                                                                RoomId            = roomId
                                                                              });

        ManagerLocator.MatchmakingManager.RemovePlayer(playerContainer.Key);
      }

      Console.WriteLine($"[#] Sent game server address to {players.Count} players");
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task<Guid> HandleResponseAsync(TcpClient tcpClient)
  {
    var response = await ReadResponseAsync(tcpClient);

    try
    {
      var roomInfo = JsonConvert.DeserializeObject<dynamic>(response);

      if (roomInfo != null) return roomInfo.RoomId;

      return Guid.Empty;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      return Guid.Empty;
    }
  }

  private async Task<string> ReadResponseAsync(TcpClient tcpClient)
  {
    var stream    = tcpClient.GetStream();
    var buffer    = new byte[1024];
    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    var response  = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    return response;
  }
}