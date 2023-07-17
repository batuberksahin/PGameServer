namespace MasterServer.Managers;

public class ManagerLocator
{
  private static readonly Lazy<MatchmakingManager> MatchmakingManagerInstance = new(() => new MatchmakingManager());
  public static           MatchmakingManager       MatchmakingManager => MatchmakingManagerInstance.Value;

  private static readonly Lazy<GameServerPingManager> GameServerPingManagerInstance =
    new(() => new GameServerPingManager());

  public static GameServerPingManager GameServerPingManager => GameServerPingManagerInstance.Value;

  private static readonly Lazy<PlayerPingManager> PlayerPingManagerInstance = new(() => new PlayerPingManager());
  public static           PlayerPingManager       PlayerPingManager => PlayerPingManagerInstance.Value;
}