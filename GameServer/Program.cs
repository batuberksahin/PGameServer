using System.Net;
using System.Net.Sockets;
using GameServer.Behaviours.MasterServerBehaviours;
using GameServer.Behaviours.PlayerBehaviours;
using GameServer.Jobs;
using NetworkLibrary;
using NetworkLibrary.Jobs;

namespace GameServer;

public static class GameServer
{
  public const short MasterServerPort = 13312;
  public static short Port;

  public static Guid ServerId { get; } = Guid.NewGuid();

  private static void Main(string[] args)
  {
    Console.Title = "Game Server";
    
    Console.Write("Enter the port number for the Game Server: ");
    if (!short.TryParse(Console.ReadLine(), out Port))
    {
      Console.WriteLine("Invalid port number. Using default port 13313.");
      
      Port = 13313;
    }
    
    ITcpServer server = new TcpServer(Port);

    server.RegisterBehaviour("server_new_room", new NewRoomBehaviour());

    server.RegisterBehaviour("player_connect",  new PlayerConnectBehaviour());
    server.RegisterBehaviour("player_ready",    new PlayerReadyBehaviour());
    server.RegisterBehaviour("player_position", new PlayerUpdatePositionBehaviour());
    server.RegisterBehaviour("player_finish",   new PlayerFinishBehaviour());

    // Jobs
    var pingTcpClient = new TcpClient();

    var jobScheduler = new JobScheduler(TimeSpan.FromSeconds(0.3));
    jobScheduler.RegisterTask(new PingJob(pingTcpClient, LocalIP.GetLocalIPv4(), MasterServerPort));

    jobScheduler.Start();

    Task.Run(() => server.StartAsync());

    var masterServerTcpClient = new TcpClient();
    Task.Run(() => SendOpenRequestToMasterServer(masterServerTcpClient));

    Console.WriteLine($"Game server ({ServerId}) started. Press Enter to stop the server...");
    Console.ReadLine();

    server.StopImmediately();
  }

  private static async Task SendOpenRequestToMasterServer(TcpClient masterServerTcpClient)
  {
    if (!masterServerTcpClient.Connected)
      await masterServerTcpClient.ConnectAsync(LocalIP.GetLocalIPv4(), MasterServerPort);

    // Create open request model
    var openRequest = new OpenRequest
                      {
                        ServerId        = ServerId,
                        ServerIpAddress = Dns.Resolve("localhost").AddressList[1].ToString(),
                        ServerPort      = Port,
                        Timestamp       = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
                      };

    // Send open request
    await Messenger.SendResponseAsync(masterServerTcpClient, "server_open", openRequest);
  }
}