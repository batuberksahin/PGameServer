using System.Net.Sockets;
using NetworkLibrary.Behaviours;

namespace MasterServer.Behaviours.GameServerBehaviour;

public class GameServerPingRequest
{
    public string ServerId;
    public string ServerPort;
}

public class GameServerPingResponse
{
    public bool Success;
    public string Message;
}

public class PingBehaviour : BehaviourBase<GameServerPingRequest, GameServerPingResponse>
{
    public override async Task<GameServerPingResponse> ExecuteBehaviourAsync(TcpClient client, GameServerPingRequest request)
    {
        Console.WriteLine($"[$] {request.ServerId} Ping received");

        return new GameServerPingResponse{ Success = true, Message = "Ping received"};
    }
}