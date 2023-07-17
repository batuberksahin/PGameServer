using System.Net.Sockets;
using GameServer.Managers;
using NetworkLibrary.Behaviours;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace GameServer.Behaviours.MasterServerBehaviours;

public class NewRoomRequest
{
  public List<Player>? Players;
}

public class NewRoomResponse
{
  public bool?   Success;
  public string? Message;

  public Guid? RoomId;
}

public class NewRoomBehaviour : BehaviourBase<NewRoomRequest, NewRoomResponse>
{
  private readonly IRepository<RepositoryLibrary.Models.GameServer> _gameServerRepository;

  public NewRoomBehaviour()
  {
    _gameServerRepository = new GameServerRepository("GameServers");
  }

  public override async Task<NewRoomResponse> ExecuteBehaviourAsync(TcpClient client, NewRoomRequest request)
  {
    try
    {
      Console.WriteLine($"[$] New room created. {request.Players?.Count}");

      var gameServer = await _gameServerRepository.GetByGuidAsync(GameServer.ServerId);

      var room = new Room
                 {
                   Id            = Guid.NewGuid(),
                   ActivePlayers = request.Players,
                   Capacity      = request.Players.Count
                 };

      if (gameServer != null) gameServer.Rooms.Add(room);
      else gameServer.Rooms = new List<Room> { room };

      await _gameServerRepository.UpdateAsync(gameServer);

      ManagerLocator.RoomManager.AddRoom(room);
      ManagerLocator.RoomManager.StartRoom(room);

      return new NewRoomResponse
             {
               Success = true,
               Message = "Room created and started successfully",

               RoomId = room.Id
             };
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }
}