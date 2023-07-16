using System;
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

namespace GameServer.Jobs
{
    public class StreamGameData : JobBase
    {
        private Room _room;
        private List<TcpClient> _clients;

        private List<Player>? _players;

        private bool _gameStarted;
        private bool _countdownStarted;
        private int _countdownTime;
        private DateTime _startTime;

        public StreamGameData(Room room, List<TcpClient> clients)
        {
            _room = room;
            _clients = clients;

            _players = room.ActivePlayers;
        }

        public override async Task RunAsync()
        {
            if (!_gameStarted)
            {
                if (AllPlayersReady())
                {
                    await StartCountdown();
                    _gameStarted = true;
                    _startTime = DateTime.UtcNow;
                }
                else
                {
                    await SendPlayersReadyStatus();
                }
            }
            else
            {
                if (_countdownStarted)
                {
                    await UpdateCountdown();
                }
                else
                {
                    await UpdateGameLogic();
                }
            }
        }

        private bool AllPlayersReady()
        {
            return (_players ?? throw new InvalidOperationException()).All(player => ManagerLocator.RoomManager.IsPlayerReady(player.Id));
        }

        private async Task StartCountdown()
        {
            _countdownTime = 3;
            _countdownStarted = true;

            while (_countdownTime > 0)
            {
                await SendCountdown();
                await Task.Delay(1000);
                _countdownTime--;
            }

            await SendGameStarted();
            _countdownStarted = false;
        }

        private async Task SendCountdown()
        {
            var countdownObject = JsonConvert.SerializeObject(new
            {
                Countdown = _countdownTime
            });
            
            foreach (var client in _clients)
            {
                await Messenger.SendResponseAsync(client, "countdown", countdownObject);
            }
        }

        private async Task SendGameStarted()
        
        {
            _startTime = DateTime.Now;

            var gameStartedObject = JsonConvert.SerializeObject(new
            {
                StartTime = _startTime
            });
            
            foreach (var client in _clients)
            {
                await Messenger.SendResponseAsync(client, "game_started", gameStartedObject);
            }
        }

        private async Task SendPlayersReadyStatus()
        {
            // combine players id and their ready status in a dictionary
            Dictionary<Guid, bool> playersReadyStatus = _players.ToDictionary(player => player.Id, player => ManagerLocator.RoomManager.IsPlayerReady(player.Id));

            var playersReadyStatusObject = JsonConvert.SerializeObject(new
            {
                PlayerStatuses = playersReadyStatus
            });
            
            foreach (var client in _clients)
            {
                await Messenger.SendResponseAsync(client, "players_status", playersReadyStatusObject);
            }
        }

        private async Task UpdateCountdown()
        {
            // Do nothing during countdown
            await Task.Delay(1000);
        }

        private async Task UpdateGameLogic()
        {
            var playerPositions = GetPlayerPositions();

            var playerPositionsObject = JsonConvert.SerializeObject(new
            {
                PlayerPositions = playerPositions
            });

            foreach (var client in _clients)
            {
                await Messenger.SendResponseAsync(client, "player_positions", playerPositionsObject);
            }

            if (GameEndConditionMet())
            {
                await FinishGame();
            }

            await Task.Delay(16);
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
            if (_gameStarted && DateTime.UtcNow.Subtract(_startTime).Minutes >= 2)
            {
                return true;
            }
            
            return _players.All(player => ManagerLocator.RoomManager.IsPlayerFinish(player.Id));
        }

        private async Task FinishGame()
        {
            var playersTime = new Dictionary<Guid, int>();

            foreach (Player player in _players)
            {
                playersTime[player.Id] = ManagerLocator.RoomManager.GetPlayerFinishTime(player.Id).Subtract(_startTime).Seconds;
            }
            
            var resultsObject = JsonConvert.SerializeObject(new
            {
                Results = playersTime
            });
            
            foreach (var client in _clients)
            {
                await Messenger.SendResponseAsync(client, "game_results", resultsObject);
            }

            ManagerLocator.RoomManager.StopRoom(_room);
        }
    }
}
