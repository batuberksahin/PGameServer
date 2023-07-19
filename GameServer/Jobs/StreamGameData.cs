﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.Managers;
using NetworkLibrary;
using NetworkLibrary.Jobs;
using Newtonsoft.Json;
using RepositoryLibrary.Models;

namespace GameServer.Jobs;

public class PlayerReadyStatus
{
  public Guid   PlayerId;
  public string PlayerName;
  public bool   IsReady;
}

public class StreamGameData : JobBase
{
  private Room            _room;
  private List<TcpClient> _clients;

  private List<Player>? _players;

  private bool     _gameStarted;
  private bool     _countdownStarted;
  private int      _countdownTime;
  private DateTime _startTime;

  private CancellationTokenSource _cancellationTokenSource;
  
  private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
                                                            {
                                                              ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                              TypeNameHandling      = TypeNameHandling.Auto,
                                                            };

  public StreamGameData(Room room)
  {
    _room    = room;
    _clients = new List<TcpClient>();

    _players = room.ActivePlayers;
    
    _cancellationTokenSource = new CancellationTokenSource();
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

      foreach (var client in _clients) await Messenger.SendResponseAsync(client, "game_started", _startTime);
    }
    catch (Exception e)
    {
      Console.Error.WriteLine(e);
      throw;
    }
  }

  private async Task SendPlayersReadyStatus()
  {
    try
    {
      var playersReadyStatus = new List<PlayerReadyStatus>();
      
      foreach (var player in _players)
      {
        var playerReadyStatus = new PlayerReadyStatus
                                {
                                  PlayerId   = player.Id,
                                  PlayerName = ManagerLocator.RoomManager.GetPlayerName(player.Id),
                                  IsReady    = ManagerLocator.RoomManager.IsPlayerReady(player.Id)
                                };

        playersReadyStatus.Add(playerReadyStatus);
      }
      
      foreach (var client in _clients)
        await Messenger.SendResponseAsync(client, "players_status", playersReadyStatus);
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
      await Messenger.SendResponseAsync(client, "player_positions", playerPositions);

    if (GameEndConditionMet()) await FinishGame();
  }

  private Dictionary<Guid, KeyValuePair<Vector3, Quaternion>> GetPlayerPositions()
  {
    Dictionary<Guid, KeyValuePair<Vector3, Quaternion>> playerPositions = new();

    foreach (var player in _players)
    {
      var position = GetPlayerPosition(player.Id);

      playerPositions[player.Id] = position;
    }

    return playerPositions;
  }

  private KeyValuePair<Vector3, Quaternion> GetPlayerPosition(Guid playerId)
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

    ManagerLocator.RoomManager.StopRoom(_room);
  }
}