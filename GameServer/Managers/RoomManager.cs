using System.Net.Sockets;
using System.Numerics;
using Amazon.Runtime.Internal.Transform;
using GameServer.Jobs;
using NetworkLibrary.Jobs;
using RepositoryLibrary;
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

  private Dictionary<Guid, string> _playerNames;

  public RoomManager()
  {
    _roomStartStatuses    = new Dictionary<Guid, bool>();
    _roomSchedulers       = new Dictionary<Guid, JobScheduler>();
    _playerNames          = new Dictionary<Guid, string>();
    _playerClients        = new Dictionary<KeyValuePair<Guid, Guid>, TcpClient>();
    _playerReadyStatuses  = new Dictionary<KeyValuePair<Guid, Guid>, bool>();
    _playerPositions      = new Dictionary<KeyValuePair<Guid, Guid>, KeyValuePair<Vector3, Quaternion>>();
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
      _playerClients.Add(new KeyValuePair<Guid, Guid>(player.ActiveRoom.Value, player.Id), client);
      
      // if not contains in playerNames
      if (!_playerNames.ContainsKey(player.Id))
        _playerNames.Add(player.Id, player.Username);
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

  public bool IsPlayerExistsInRoom(Guid playerId)
  {
    return _playerClients.Keys.Any(x => x.Value == playerId);
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
    try
    {
      var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;
      return _playerReadyStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)];
    }
    catch
    {
      return false;
    }
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

    var jobScheduler = new JobScheduler(TimeSpan.FromMilliseconds(16));

    jobScheduler.RegisterTask(new StreamGameData(room));

    jobScheduler.Start();
    
    Console.WriteLine($"[RoomManager] Room {room.Id} started.");

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
  
  public List<TcpClient> GetClientsInRoom(Room room)
  {
    return _playerClients.Where(x => x.Key.Key == room.Id).Select(x => x.Value).ToList();
  }

  public void UpdatePlayerPositionAndRotation(Guid playerId, float X, float Y, float Z, Quaternion rotation)
  {
    try
    {
      var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

      var room = _rooms.FirstOrDefault(x => x.Id == roomId);

      var player = room?.ActivePlayers?.FirstOrDefault(x => x.Id == playerId);

      var position = new Vector3(X, Y, Z);

      _playerPositions[new KeyValuePair<Guid, Guid>(roomId, playerId)] =
        new KeyValuePair<Vector3, Quaternion>(position, rotation);
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  public PlayerPosition GetPlayerPositionAndRotation(Guid playerId)
  {
    // if doesnt exist in _playerPositions return default
    if (!_playerPositions.ContainsKey(new KeyValuePair<Guid, Guid>(_playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key, playerId)))
      return new PlayerPosition
             {
               PlayerId = playerId,
               Position = new List<float>
                          {
                            0,
                            0,
                            0,
                          },
               Rotation = new List<float>
                          {
                            0,
                            0,
                            0,
                            0,
                          }
             };
    
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;
    
    var position     = _playerPositions[new KeyValuePair<Guid, Guid>(roomId, playerId)].Key;
    var positionList = new List<float> {position.X, position.Y, position.Z};
    var rotation     = _playerPositions[new KeyValuePair<Guid, Guid>(roomId, playerId)].Value;
    
    return new PlayerPosition
           {
             PlayerId = playerId,
             Position = positionList,
             Rotation = new List<float>
                        {
                          rotation.X,
                          rotation.Y,
                          rotation.Z,
                          rotation.W
                        }
           };
  }

  public void FinishPlayer(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    Console.WriteLine($"[RoomManager] Player {playerId} finished in room {roomId}.");
    
    _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)] =
      new KeyValuePair<DateTime, bool>(DateTime.Now, true);
  }

  public bool IsPlayerFinish(Guid playerId)
  {
    if (!_playerFinishStatuses.ContainsKey(new KeyValuePair<Guid, Guid>(_playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key, playerId)))
      return false;

    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    return _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)].Value;
  }

  public DateTime GetPlayerFinishTime(Guid playerId)
  {
    var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

    // if doesnt exist return default
    if (!_playerFinishStatuses.ContainsKey(new KeyValuePair<Guid, Guid>(roomId, playerId)))
      return DateTime.Now;
    
    return _playerFinishStatuses[new KeyValuePair<Guid, Guid>(roomId, playerId)].Key;
  }

  public string GetPlayerName(Guid playerId)
  {
    return _playerNames.TryGetValue(playerId, out var name) ? name : "Unknown";
  }

  public Vector3  GetPlayerPositionToOrder(Guid playerId)
  {
    if (!_playerPositions.ContainsKey(new KeyValuePair<Guid, Guid>(_playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key, playerId)))
    {
      var roomId = _playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key;

      var room = _rooms.FirstOrDefault(x => x.Id == roomId);

      var player = room?.ActivePlayers?.FirstOrDefault(x => x.Id == playerId);

      var order = room?.ActivePlayers?.IndexOf(player) ?? 0;

      _playerPositions.Add(new KeyValuePair<Guid, Guid>(roomId, playerId),
        new KeyValuePair<Vector3, Quaternion>(new Vector3(order * 2, 0, 0), Quaternion.Identity));
      
      return new Vector3(order * 2, 0, 0);
    }
    
    return _playerPositions[new KeyValuePair<Guid, Guid>(_playerClients.Keys.FirstOrDefault(x => x.Value == playerId).Key, playerId)].Key;
  }
}