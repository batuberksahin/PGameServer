using System.Net.Sockets;
using MasterServer.Managers;
using NetworkLibrary.Behaviours;

namespace MasterServer.Behaviours.GameServerBehaviour;

public class GameServerPingRequest
{
    public Guid ServerId;
    public string? ServerPort;
}

public class GameServerPingResponse
{
    public bool? Success;
    public string? Message;
}

public class GameServerPingBehaviour : BehaviourBase<GameServerPingRequest, GameServerPingResponse>
{
    public override Task<GameServerPingResponse> ExecuteBehaviourAsync(TcpClient client, GameServerPingRequest request)
    {
        ManagerLocator.GameServerPingManager.PingGameServer(request.ServerId);
        
        return Task.FromResult(new GameServerPingResponse{ Success = true, Message = "Ping received"});
    }
}