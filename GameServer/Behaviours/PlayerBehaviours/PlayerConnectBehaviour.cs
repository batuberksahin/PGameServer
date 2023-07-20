using System.Net.Sockets;
using GameServer.Managers;
using NetworkLibrary.Behaviours;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace GameServer.Behaviours.PlayerBehaviours;

public class ConnectRequest
{
  public Guid PlayerId;

  public Guid RoomId;
}

public class ConnectResponse
{
  public bool?   Success;
  public string? Message;
}

public class PlayerConnectBehaviour : BehaviourBase<ConnectRequest, ConnectResponse>
{
  private readonly IRepository<Player> _playerRepository;

  public PlayerConnectBehaviour()
  {
    _playerRepository = new PlayerRepository("Players");
  }

  public override async Task<ConnectResponse> ExecuteBehaviourAsync(TcpClient client, ConnectRequest request)
  {
    try
    {
      var player = await _playerRepository.GetByGuidAsync(request.PlayerId);

      Console.WriteLine($"received connection request from {request.RoomId} to {request.PlayerId}");
      player.ActiveRoom = request.RoomId;

      await _playerRepository.UpdateAsync(player);

      ManagerLocator.RoomManager.AddPlayer(player, client);

      return new ConnectResponse
             {
               Success = true,
               Message = "Connection established"
             };
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }
}