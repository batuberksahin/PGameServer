namespace GameServer.Managers;

public class ManagerLocator
{
  private static readonly Lazy<RoomManager> RoomManagerInstance = new(() => new RoomManager());

  public static RoomManager RoomManager => RoomManagerInstance.Value;
}