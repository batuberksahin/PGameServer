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
    server.RegisterBehaviour("player_connect",   new PlayerConnectBehaviour());
    server.RegisterBehaviour("player_ping",      new PlayerPingBehaviour());
    server.RegisterBehaviour("player_play_game", new PlayerPlayGameBehaviour());

    // Game Server Behaviours
    server.RegisterBehaviour("server_ping", new GameServerPingBehaviour());
    server.RegisterBehaviour("server_open", new GameServerOpenBehaviour());

    // Jobs
    var jobScheduler = new JobScheduler(TimeSpan.FromSeconds(2.0));
    jobScheduler.RegisterTask(new MatchmakingJob());

    jobScheduler.Start();

    Task.Run(() => server.StartAsync());

    Console.WriteLine("Server started. Press Enter to stop the server...");
    Console.ReadLine();

    server.StopImmediately();
  }
}