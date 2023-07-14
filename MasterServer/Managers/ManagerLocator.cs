namespace MasterServer.Managers;

public class ManagerLocator
{
    private static readonly Lazy<MatchmakingManager> MatchmakingManagerInstance =
        new Lazy<MatchmakingManager>(() => new MatchmakingManager());
    public static MatchmakingManager MatchmakingManager => MatchmakingManagerInstance.Value;
    
    private static readonly Lazy<GameServerPingManager> GameServerPingManagerInstance =
        new Lazy<GameServerPingManager>(() => new GameServerPingManager());
    public static GameServerPingManager GameServerPingManager => GameServerPingManagerInstance.Value;
    
    private static readonly Lazy<PlayerPingManager> PlayerPingManagerInstance =
        new Lazy<PlayerPingManager>(() => new PlayerPingManager());
    public static PlayerPingManager PlayerPingManager => PlayerPingManagerInstance.Value;
}