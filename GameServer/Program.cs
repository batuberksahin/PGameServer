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
    public const short MasterServerPort = 8888;
    public const short Port = 1310;
    
    public static Guid ServerId { get; } = Guid.NewGuid();
    
    private static void Main(string[] args)
    {
        ITcpServer server = new TcpServer(Port);
        
        server.RegisterBehaviour("server_new_room", new NewRoomBehaviour());
        
        server.RegisterBehaviour("player_connect", new PlayerConnectBehaviour());
        server.RegisterBehaviour("player_ready", new PlayerReadyBehaviour());
        server.RegisterBehaviour("player_position", new PlayerUpdatePositionBehaviour());
        server.RegisterBehaviour("player_finish", new PlayerFinishBehaviour());
        
        // Jobs
        TcpClient pingTcpClient = new TcpClient();
        
        JobScheduler jobScheduler = new JobScheduler(TimeSpan.FromSeconds(0.3));
        jobScheduler.RegisterTask(new PingJob(pingTcpClient, "127.0.0.1", MasterServerPort));
        
        jobScheduler.Start();
        
        Task.Run(() => server.StartAsync());

        TcpClient masterServerTcpClient = new TcpClient();
        Task.Run(() => SendOpenRequestToMasterServer(masterServerTcpClient));

        Console.WriteLine($"Game server ({ServerId}) started. Press Enter to stop the server...");
        Console.ReadLine();
        
        server.StopImmediately();
    }

    private static async Task SendOpenRequestToMasterServer(TcpClient masterServerTcpClient)
    {
        if (!masterServerTcpClient.Connected)
            await masterServerTcpClient.ConnectAsync("127.0.0.1", MasterServerPort);

        // Create open request model
        var openRequest = new OpenRequest
        {
            ServerId = ServerId,
            ServerIpAddress = Dns.Resolve("localhost").AddressList[1].ToString(),
            ServerPort = Port,
            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
        };

        // Send open request
        await Messenger.SendResponseAsync(masterServerTcpClient, "server_open", openRequest);
    }
}