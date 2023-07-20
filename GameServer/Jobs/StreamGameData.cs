using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.Managers;
using NetworkLibrary;
using NetworkLibrary.Jobs;
using Newtonsoft.Json;
using RepositoryLibrary;
using RepositoryLibrary.Models;

namespace GameServer.Jobs;

public class PlayerReadyStatus
{
  public Guid    PlayerId;
  public string  PlayerName;
  public bool    IsReady;
  public List<float> Position;
}

public class PlayerPosition
{
  public Guid    PlayerId;
  public List<float> Position;
  public List<float> Rotation;
}

public class StreamGameData : JobBase
{
  private Room            _room;
  private List<TcpClient> _clients;

  private List<Player>? _players;

  private bool     _gameStarted;
  private bool     _countdownStarted;
  private bool     _playerReadyStatusSendOnce;
  private int      _countdownTime;
  private DateTime _startTime;

  private CancellationTokenSource _cancellationTokenSource;
  
  private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
                                                            {
                                                              ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                              TypeNameHandling      = TypeNameHandling.Auto,
                                                            };

  private readonly IRepository<GameRecord> _gameRecordsRepository;
  private readonly IRepository<Player> _playersRepository;

  public StreamGameData(Room room)
  {
    _room    = room;
    _clients = new List<TcpClient>();

    _players = room.ActivePlayers;

    _playerReadyStatusSendOnce = false;
    
    _cancellationTokenSource = new CancellationTokenSource();
    
    _gameRecordsRepository = new GameRecordRepository("GameRecords");
    _playersRepository     = new PlayerRepository("Players");
  }

  public override Task StartAsync()
  {
    Task.Run(async () =>
             {
               while (!_cancellationTokenSource.Token.IsCancellationRequested)
               {
                 _clients = ManagerLocator.RoomManager.GetClientsInRoom(_room);
                 
                 await Task.Delay(1000);
               }
             });
    
    return Task.CompletedTask;
  }

  public override async Task RunAsync()
  {
    try
    {
      if (!_gameStarted)
      {
        if (AllPlayersReady())
        {
          if (!_playerReadyStatusSendOnce)
          {
            await SendPlayersReadyStatus();
            
            return;
          }
          
          await StartCountdown();
          _gameStarted = true;
          _startTime   = DateTime.UtcNow;
        }
        else
        {
          await SendPlayersReadyStatus();
        }
      }
      else
      {
        if (!_playerReadyStatusSendOnce)
        {
          await SendPlayersReadyStatus();
            
          return;
        }
        
        if (_countdownStarted)
          await UpdateCountdown();
        else
          await UpdateGameLogic();
      }
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private bool AllPlayersReady()
  {
    try
    {
      return (_players ?? throw new InvalidOperationException()).All(player =>
                                                                       ManagerLocator.RoomManager
                                                                                     .IsPlayerReady(player.Id));
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task StartCountdown()
  {
    try
    {
      _countdownTime    = 4;
      _countdownStarted = true;

      while (_countdownTime > 0)
      {
        await SendCountdown();
        await Task.Delay(3000);
        _countdownTime--;
      }

      await SendGameStarted();
      _countdownStarted = false;
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task SendCountdown()
  {
    try
    {
      foreach (var client in _clients) await Messenger.SendResponseAsync(client, "countdown", _countdownTime);
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task SendGameStarted()
  {
    try
    {
      _startTime = DateTime.Now;

      foreach (var client in _clients) await Messenger.SendResponseAsync(client, "game_started", new
                                                                                                 {
                                                                                                   StartTime = _startTime,
                                                                                                   Players = GetPlayersReadyStatus()
                                                                                                 });
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private List<PlayerReadyStatus> GetPlayersReadyStatus()
  {
    var playersReadyStatus = new List<PlayerReadyStatus>();
      
    foreach (var player in _players)
    {
      var playerPositionVector3 = ManagerLocator.RoomManager.GetPlayerPositionToOrder(player.Id);
        
      // map vector3 to float list
      var playerPosition = new List<float>
                           {
                             playerPositionVector3.X,
                             playerPositionVector3.Y,
                             playerPositionVector3.Z
                           };
        
      var playerReadyStatus = new PlayerReadyStatus
                              {
                                PlayerId   = player.Id,
                                PlayerName = ManagerLocator.RoomManager.GetPlayerName(player.Id),
                                IsReady    = ManagerLocator.RoomManager.IsPlayerReady(player.Id),
                                Position   = playerPosition
                              };

      playersReadyStatus.Add(playerReadyStatus);
    }

    return playersReadyStatus;
  }
  
  private async Task SendPlayersReadyStatus()
  {
    try
    {
      foreach (var client in _clients)
        await Messenger.SendResponseAsync(client, "players_status", GetPlayersReadyStatus());
      
      _playerReadyStatusSendOnce = true;
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task UpdateCountdown()
  {
    // Do nothing during countdown
    await Task.Delay(3000);
  }

  private async Task UpdateGameLogic()
  {
    var playerPositions = GetPlayerPositions();

    foreach (var client in _clients)
    {
      Task.Run(() => Messenger.SendResponseAsync(client, "player_positions", playerPositions));
    }

    if (GameEndConditionMet()) await FinishGame();
  }

  private List<PlayerPosition> GetPlayerPositions()
  {
    List<PlayerPosition> playerPositions = new List<PlayerPosition>();

    foreach (var player in _players)
    {
      var playerPosition = GetPlayerPosition(player.Id);

      playerPositions.Add(playerPosition);
    }

    return playerPositions;
  }

  private PlayerPosition GetPlayerPosition(Guid playerId)
  {
    return ManagerLocator.RoomManager.GetPlayerPositionAndRotation(playerId);
  }

  private bool GameEndConditionMet()
  {
    // if game started and time is passed 2 minutes force to end
    if (_gameStarted && DateTime.UtcNow.Subtract(_startTime).Minutes >= 2) return true;

    return _players.All(player => ManagerLocator.RoomManager.IsPlayerFinish(player.Id));
  }

  private async Task FinishGame()
  {
    Console.WriteLine($"{_room.Id} game finished");

    var playersTime = new Dictionary<Guid, int>();

    foreach (var player in _players)
      playersTime[player.Id] = ManagerLocator.RoomManager.GetPlayerFinishTime(player.Id).Subtract(_startTime).Seconds;

    foreach (var client in _clients) await Messenger.SendResponseAsync(client, "game_results", playersTime);

    // Update players score from _playerRepository by rank. for example if game has 3 players and first player finished
    // first, second player finished second and third player finished third, then first player will get 3 points,
    // second player will get 2 points and third player will get 1 point
    foreach (var player in _players)
    {
      var playerRank = playersTime.OrderBy(pair => pair.Value)
                                  .Select(pair => pair.Key)
                                  .ToList()
                                  .IndexOf(player.Id) + 1;
      
      var playerScore = player.Score;

      player.Score = playerScore + (4 - playerRank);
     
      await _playersRepository.UpdateAsync(player);
    }
    
    
    await _gameRecordsRepository.SaveAsync(new GameRecord
                                           {
                                             Id           = Guid.NewGuid(),
                                             GameServerId = GameServer.ServerId,
                                             RoomId       = _room.Id,
                                             PlayerIdsOrderedByRank = playersTime.OrderBy(pair => pair.Value)
                                                                                 .Select(pair => pair.Key)
                                                                                 .ToList()
                                           });
    
    ManagerLocator.RoomManager.StopRoom(_room);
  }
}