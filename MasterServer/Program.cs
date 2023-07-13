using MasterServer.Behaviours.GameServerBehaviour;
using MasterServer.Behaviours.PlayerBehaviours;
using MasterServer.Jobs;
using NetworkLibrary;
using NetworkLibrary.Jobs;

namespace MasterServer;

public static class MasterServer
{
    private static void Main(string[] args)
    {
        ITcpServer server = new TcpServer(8888);
        
        // Player Behaviours
        server.RegisterBehaviour("player_connect", new PlayerConnectBehaviour());
        
        // Game Server Behaviours
        server.RegisterBehaviour("server_ping", new PingBehaviour());

        // Jobs
        JobScheduler jobScheduler = new JobScheduler(TimeSpan.FromSeconds(2.0));
        jobScheduler.RegisterTask(new MatchmakingJob());
        
        jobScheduler.Start();

        Task.Run(() => server.StartAsync());

        Console.WriteLine("Server started. Press Enter to stop the server...");
        Console.ReadLine();
        
        server.StopImmediately();
    }
}
