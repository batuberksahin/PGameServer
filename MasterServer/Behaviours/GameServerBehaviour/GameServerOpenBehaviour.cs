using System.Net.Sockets;
using MasterServer.Managers;
using NetworkLibrary.Behaviours;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace MasterServer.Behaviours.GameServerBehaviour;

public class GameServerOpenRequest
{
    public Guid ServerId;
    
    public string? ServerIpAddress;
    public int ServerPort;
}

public class GameServerOpenResponse
{
    public bool? Success;
    public string? Message;
}

public class GameServerOpenBehaviour : BehaviourBase<GameServerOpenRequest, GameServerOpenResponse>
{
    private readonly IRepository<GameServer> _gameServerRepository;
    
    public GameServerOpenBehaviour()
    {
        _gameServerRepository = new GameServerRepository("GameServers");
    }

    public override async Task<GameServerOpenResponse> ExecuteBehaviourAsync(TcpClient client, GameServerOpenRequest request)
    {
        // TODO: burda bug olabilir gameServer databaseden çektiğim için kayıt etmeyebilir
        try
        {
            GameServer gameServer = await _gameServerRepository.GetByGuidAsync(request.ServerId);

            GameServerOpenResponse gameServerOpenResponse = new()
            {
                Success = true,
                Message = ""
            };

            if (gameServer == null)
            {
                gameServer = new GameServer
                {
                    Id = request.ServerId,
                    Port = request.ServerPort,
                    Rooms = new List<Room>()
                };

                await _gameServerRepository.SaveAsync(gameServer);

                gameServerOpenResponse.Message = "Game server added to database";
            }
            else
            {
                gameServerOpenResponse.Message = "Game server already exists in database";
            }

            ManagerLocator.MatchmakingManager.AddGameServer(gameServer, client);

            Console.WriteLine(
                $"[#] Game Server \"{request.ServerId}\" connected. {request.ServerIpAddress}:{request.ServerPort}");

            return gameServerOpenResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}