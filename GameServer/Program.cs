using System.Net.Sockets;
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
        
        Task.Run(() => server.StartAsync());

        // Jobs
        TcpClient pingTcpClient = new TcpClient();
        JobScheduler jobScheduler = new JobScheduler(TimeSpan.FromSeconds(0.3));
        jobScheduler.RegisterTask(new PingJob(pingTcpClient, "127.0.0.1", MasterServerPort));
        
        jobScheduler.Start();
        
        Console.WriteLine($"Game server ({ServerId}) started. Press Enter to stop the server...");
        Console.ReadLine();
        
        server.StopImmediately();
    }
}