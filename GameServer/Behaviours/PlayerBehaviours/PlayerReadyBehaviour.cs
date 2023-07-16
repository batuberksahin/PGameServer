using System.Net.Sockets;
using GameServer.Managers;
using NetworkLibrary.Behaviours;

namespace GameServer.Behaviours.PlayerBehaviours;

public class PlayerReadyRequest
{
    public Guid PlayerId;
}

public class PlayerReadyResponse
{
    public bool? Success;
    public string? Message;
}

public class PlayerReadyBehaviour : BehaviourBase<PlayerReadyRequest, PlayerReadyResponse>
{
    public override Task<PlayerReadyResponse> ExecuteBehaviourAsync(TcpClient client, PlayerReadyRequest request)
    {
        ManagerLocator.RoomManager.ReadyPlayer(request.PlayerId);
        
        return Task.FromResult(new PlayerReadyResponse
        {
            Success = true,
            Message = "Player is ready"
        });
    }
}