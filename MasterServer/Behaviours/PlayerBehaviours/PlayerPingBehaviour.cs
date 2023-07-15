using System.Net.Sockets;
using MasterServer.Managers;
using NetworkLibrary.Behaviours;

namespace MasterServer.Behaviours.PlayerBehaviours;

public class PlayerPingRequest
{
    public Guid PlayerId;
}

public class PlayerPingResponse
{
    public bool? Success;
    public string? Message;
}


public class PlayerPingBehaviour : BehaviourBase<PlayerPingRequest, PlayerPingResponse>
{
    public override async Task<PlayerPingResponse> ExecuteBehaviourAsync(TcpClient client, PlayerPingRequest request)
    {
        ManagerLocator.PlayerPingManager.PingPlayer(request.PlayerId);

        return await Task.FromResult(new PlayerPingResponse{ Success = true, Message = "Ping received"});
    }
}