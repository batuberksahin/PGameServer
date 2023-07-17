using System.Net.Sockets;
using GameServer.Managers;
using NetworkLibrary.Behaviours;

namespace GameServer.Behaviours.PlayerBehaviours;

public class PlayerFinishRequest
{
  public Guid PlayerId;
}

public class PlayerFinishResponse
{
  public bool?   Success;
  public string? Message;
}

public class PlayerFinishBehaviour : BehaviourBase<PlayerFinishRequest, PlayerFinishResponse>
{
  public override Task<PlayerFinishResponse> ExecuteBehaviourAsync(TcpClient client, PlayerFinishRequest request)
  {
    ManagerLocator.RoomManager.FinishPlayer(request.PlayerId);

    return Task.FromResult(new PlayerFinishResponse
                           {
                             Success = true,
                             Message = "Player finished"
                           });
  }
}