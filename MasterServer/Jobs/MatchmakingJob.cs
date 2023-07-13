using System.Net.Sockets;
using NetworkLibrary.Jobs;

namespace MasterServer.Jobs;

public class MatchmakingJob : JobBase
{
    private const int MaxPlayers = 2;
    
    public override Task RunAsync()
    {
        throw new NotImplementedException();
    }
}