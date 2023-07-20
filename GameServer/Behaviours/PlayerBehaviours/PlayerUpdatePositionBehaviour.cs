using System.Net.Sockets;
using System.Numerics;
using GameServer.Managers;
using NetworkLibrary.Behaviours;

namespace GameServer.Behaviours.PlayerBehaviours;

public class PlayerUpdatePositionRequest
{
  public Guid       PlayerId;
  public float    PositionX;
  public float    PositionY;
  public float    PositionZ;
  public Quaternion Rotation;
}

public class PlayerUpdatePositionResponse
{
  public bool?   Success;
  public string? Message;
}

public class PlayerUpdatePositionBehaviour : BehaviourBase<PlayerUpdatePositionRequest, PlayerUpdatePositionResponse>
{
  public override Task<PlayerUpdatePositionResponse> ExecuteBehaviourAsync(
    TcpClient client, PlayerUpdatePositionRequest request)
  {
    ManagerLocator.RoomManager.UpdatePlayerPositionAndRotation(request.PlayerId, request.PositionX, request.PositionY, request.PositionZ,
                                                               request.Rotation);

    return Task.FromResult(new PlayerUpdatePositionResponse
                           {
                             Success = true,
                             Message = "Player position updated"
                           });
  }
}