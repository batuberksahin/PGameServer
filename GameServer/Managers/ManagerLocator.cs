namespace GameServer.Managers;

public class ManagerLocator
{
    private static readonly Lazy<RoomManager> RoomManagerInstance =
        new Lazy<RoomManager>(() => new RoomManager());

    public static RoomManager RoomManager => RoomManagerInstance.Value;
}