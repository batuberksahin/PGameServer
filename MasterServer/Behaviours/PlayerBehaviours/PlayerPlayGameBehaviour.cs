using System.Net.Sockets;
using MasterServer.Managers;
using NetworkLibrary.Behaviours;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace MasterServer.Behaviours.PlayerBehaviours;

public class PlayerPlayGameRequest
{
    public Guid PlayerId;
}

public class PlayerPlayGameResponse
{
    public bool? Success;
    public string? Message;
}

public class PlayerPlayGameBehaviour : BehaviourBase<PlayerPlayGameRequest, PlayerPlayGameResponse>
{
    private readonly IRepository<Player> _playerRepository;

    public PlayerPlayGameBehaviour()
    {
        _playerRepository = new PlayerRepository("Players");
    }

    public override async Task<PlayerPlayGameResponse> ExecuteBehaviourAsync(TcpClient client, PlayerPlayGameRequest request)
    {
        Player player = await _playerRepository.GetByGuidAsync(request.PlayerId);
        
        ManagerLocator.MatchmakingManager.AddPlayer(player, client);
        
        var response = new PlayerPlayGameResponse
        {
            Success = true,
            Message = "Player added to matchmaking queue",
        };

        return response;
    }
}