using System.Net.Sockets;
using NetworkLibrary.Behaviours;

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
    public override Task<PlayerPlayGameResponse> ExecuteBehaviourAsync(TcpClient client, PlayerPlayGameRequest request)
    {
        throw new NotImplementedException();
    }
}