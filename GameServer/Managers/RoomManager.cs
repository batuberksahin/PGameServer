using System.Net.Sockets;
using System.Numerics;
using Amazon.Runtime.Internal.Transform;
using GameServer.Jobs;
using NetworkLibrary.Jobs;
using RepositoryLibrary.Models;

namespace GameServer.Managers;

public class RoomManager
{
  private List<Room>                     _rooms;
  private Dictionary<Guid, bool>         _roomStartStatuses;
  private Dictionary<Guid, JobScheduler> _roomSchedulers;

  // Key: RoomId, Value: PlayerId
  private Dictionary<KeyValuePair<Guid, Guid>, TcpClient> _playerClients;
  private Dictionary<KeyValuePair<Guid, Guid>, bool>      _playerReadyStatuses;

  private Dictionary<KeyValuePair<Guid, Guid>, KeyValuePair<Vector3, Quaternion>> _playerPositions;

  private Dictionary<KeyValuePair<Guid, Guid>, KeyValuePair<DateTime, bool>> _playerFinishStatuses;

  public RoomManager()
  {
    _roomStartStatuses = new Dictionary<Guid, bool>();
    _roomSchedulers    = new Dictionary<Guid, JobScheduler>();

    _playerClients       = new Dictionary<KeyValuePair<Guid, Guid>, TcpClient>();
    _playerReadyStatuses = new Dictionary<KeyValuePair<Guid, Guid>, bool>();

    _playerPositions = new Dictionary<KeyValuePair<Guid, Guid>, KeyValuePair<Vector3, Quaternion>>();

    _playerFinishStatuses = new Dictionary<KeyValuePair<Guid, Guid>, KeyValuePair<DateTime, bool>>();

    _rooms = new List<Room>();
  }

  public void AddRoom(Room room)
  {
    _rooms.Add(room);

    _roomStartStatuses.Add(room.Id, false);
  }

  public void RemoveRoom(Room room)
  {
    _rooms.Remove(room);

    _roomStartStatuses.Remove(room.Id);
  }

  public void AddPlayer(Player player, TcpClient client)
  {
    try
    {
      if (player.ActiveRoom != null)
        _playerClients.Add(new KeyValuePair<Guid, Guid>(player.ActiveRoom.Value, player.Id), client);

      _playerReadyStatuses.Add(new KeyValuePair<Guid, Guid>(player.ActiveRoom.Value, player.Id), false);
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  public void RemovePlayer(Player player)
  {
    try
    {
      if (player.ActiveRoom != null)
        _playerClients.Remove(new KeyValuePair<Guid, Guid>(player.ActiveRoom.Value, player.Id));

      _playerReadyStatuses.Remove(new KeyValuePair<Guid, Guid>(player.ActiveRoom.Value, player.Id));
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  public void ReadyPlayer(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    _playerReadyStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)] = true;
  }

  public void UnreadyPlayer(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    _playerReadyStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)] = false;
  }

  public bool IsPlayerReady(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    return _playerReadyStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)];
  }

  public bool IsRoomReady(Room room)
  {
    var playersInRoom      = room.ActivePlayers?.Count ?? 0;
    var readyPlayersInRoom = _playerReadyStatuses.Count(x => x.Value);

    return playersInRoom == readyPlayersInRoom;
  }

  public bool IsRoomStarted(Room room)
  {
    return _roomStartStatuses[room.Id];
  }

  public void StartRoom(Room room)
  {
    _roomStartStatuses[room.Id] = true;

    var jobScheduler = new JobScheduler(TimeSpan.FromSeconds(1.0 / 60.0));

    List<TcpClient> clients = new();

    foreach (var ids in _playerClients.Keys)
      if (ids.Key == room.Id)
        clients.Add(_playerClients[ids]);

    jobScheduler.RegisterTask(new StreamGameData(room, clients));

    jobScheduler.Start();

    _roomSchedulers.Add(room.Id, jobScheduler);
  }

  public void StopRoom(Room room)
  {
    _roomStartStatuses[room.Id] = false;

    _roomSchedulers[room.Id].Stop();
  }

  public List<Room> GetRooms()
  {
    return _rooms;
  }

  public List<Player> GetPlayersInRoom(Room room)
  {
    return room.ActivePlayers ?? new List<Player>();
  }

  public void UpdatePlayerPositionAndRotation(Guid playerId, Vector3 position, Quaternion rotation)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    var room = _rooms.FirstOrDefault(x => x.Id == roomId);

    var player = room?.ActivePlayers?.FirstOrDefault(x => x.Id == playerId);

    _playerPositions[new KeyValuePair<Guid, Guid>(roomId, playerId)] =
      new KeyValuePair<Vector3, Quaternion>(position, rotation);
  }

  public KeyValuePair<Vector3, Quaternion> GetPlayerPositionAndRotation(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    return _playerPositions[new KeyValuePair<Guid, Guid>(roomId, playerId)];
  }

  public void FinishPlayer(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)] =
      new KeyValuePair<DateTime, bool>(DateTime.Now, true);
  }

  public bool IsPlayerFinish(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    return _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)].Value;
  }

  public DateTime GetPlayerFinishTime(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    return _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)].Key;
  }
}