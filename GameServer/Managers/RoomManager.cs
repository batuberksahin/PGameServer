using RepositoryLibrary.Models;

namespace GameServer.Managers;

public class RoomManager
{
    private List<Room> _rooms;

    public RoomManager()
    {
        _rooms = new List<Room>();
    }
    
    public void AddRoom(Room room)
    {
        _rooms.Add(room);
    }
    
    public void RemoveRoom(Room room)
    {
        _rooms.Remove(room);
    }
}