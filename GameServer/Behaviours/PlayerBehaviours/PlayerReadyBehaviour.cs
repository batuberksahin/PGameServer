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
  public bool?   Success;
  public string? Message;
}

public class PlayerReadyBehaviour : BehaviourBase<PlayerReadyRequest, PlayerReadyResponse>
{
  public override async Task<PlayerReadyResponse> ExecuteBehaviourAsync(TcpClient client, PlayerReadyRequest request)
  {
    while (!ManagerLocator.RoomManager.IsPlayerExistsInRoom(request.PlayerId))
    {
      await Task.Delay(100);
    }
    
    await Task.Delay(new Random().Next(500, 3000));

    ManagerLocator.RoomManager.ReadyPlayer(request.PlayerId);

    return new PlayerReadyResponse
                           {
                             Success = true,
                             Message = "Player is ready"
                           };
  }
}