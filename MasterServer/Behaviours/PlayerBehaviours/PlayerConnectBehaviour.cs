using System.Net;
using System.Net.Sockets;
using NetworkLibrary.Behaviours;
using NetworkLibrary.Middlewares;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace MasterServer.Behaviours.PlayerBehaviours;

public class ConnectRequest
{
  public string? Username;
  public string? IpAddress;
}

public class ConnectResponse
{
  public bool?   Success;
  public string? Message;

  public Guid                     PlayerId;
  public Dictionary<string, int> Leaderboard;
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
      var  player = await _playerRepository.GetByUsernameAsync(request.Username ?? string.Empty);
      Guid playerId;

      if (player == null)
      {
        playerId = Guid.NewGuid();

        player = new Player
                 {
                   Id         = playerId,
                   Username   = request.Username,
                   Score      = 0,
                   ActiveRoom = Guid.Empty
                 };

        await _playerRepository.SaveAsync(player);
      }

      playerId = player.Id;

      Console.WriteLine($"[$] Player \"{request.Username}\" connected. {playerId}");

      var response = new ConnectResponse
                     {
                       Success  = true,
                       Message  = "Connected successfully",
                       PlayerId = playerId,
                        Leaderboard = (await _playerRepository.GetAllAsync())
                                      .OrderByDescending(x => x.Score)
                                      .Take(10)
                                      .ToDictionary(x => x.Username, x => x.Score)
                     };

      return response;
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }
}