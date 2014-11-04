using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Xna.Framework;
using System.Timers;


namespace SignalRGameServer
{
    [HubName("GameHub")]
    public class GameHub : Hub
    {
        public static int _playerCounter = 0;
        public static int _playersPlaying = 0;
        public static Dictionary<int, Player> _players = new Dictionary<int, Player>();
        public static Dictionary<int, Collectable> _collectables = new Dictionary<int, Collectable>();
        public static Timer _startTime;
        public static bool _gameStarted = false;
        public static bool _gameOver = false;

        public void joinServer()
        {
            if (!_gameStarted)
            {
                //_playerCounter++;
                int count = _players.Count;
                // Player initial position has to be set on the server as this will be needed in 
                // others
                _players.Add(count, new Player(count, randomPosition(600,400)));
                // Inform the Caller that the player has joined the game
                Clients.Caller.JoinedServer(_players[count]);
                // Inform other clients that this player has joined
                Clients.Others.AddPlayer(_players[count]);
                for (int i = 0; i < _players.Count - 1; i++)
                {
                    // Inform the caller of all the other players who have joined
                    Clients.Caller.AddPlayer(_players[i]);
                }
                _playersPlaying++;
                if (_playersPlaying > 1)
                {
                    _startTime = new Timer(10000);
                    _startTime.Elapsed += _startTime_Elapsed;
                    _startTime.Start();
                    Clients.All.TimerOn(_startTime.Interval);
                }
            }
        }

        void _startTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            _startTime.Stop();
            Clients.All.Accept(createCollectables());
            Clients.All.StartGame();
            _gameStarted = true;
        }

        private Dictionary<int,Collectable> createCollectables()
        {
            Random r = new Random();
            int collectableCount = r.Next(3, 10);
            int[] possibles = { 10, 20, 30 };
            if(_collectables.Count == 0)
                for (int i = 0; i < collectableCount; i++)
                    // Add a collectable to the collection with a random position and a random possible value
                    _collectables.Add(i,
                        new Collectable(i, new Vector2(r.Next(40, 1000), r.Next(40, 700)),
                            possibles[r.Next(0, 2)]
                        ));
            return _collectables;
        }

        public void Collected(Player p, Collectable c)
        {
            _players[p.PlayerID].Score += c.Value;
            _collectables[c.Id].Collected = true;
            // if all collected then report the final score
            if (_collectables.Select(col => col.Value.Collected == false).Count() == 0)
            {
                Clients.All.PresentScoreBoard(_players.OrderBy(pl => pl.Value.Score));
                gameReset();
            }
                
        }

        private void gameReset()
        {
            throw new NotImplementedException();
        }

        // depreciated by dictionary
        public void NewPlayer(Player p)
        {
            if(_playersPlaying > 0)
                Clients.Others.AddPlayer(p);
        }

        public void DeletePlayer(Player p)
        {
            if (_players.Count > 0)
            {
                Clients.Others.DeleteClientPlayerFromOthers(p);
               _players.Remove(p.PlayerID);
               _playersPlaying--;
            }
            else // reset the game
            {
                _gameStarted = false;
                _playerCounter = 0;
                _collectables = new Dictionary<int, Collectable>();
                _players = new Dictionary<int, Player>();
            }
        }

        public void movePlayer(Player p)
        {
                _players[p.PlayerID].Position = p.Position;
                Clients.Others.UpdateOtherPlayerPosition(p);
        }

        public Vector2 randomPosition(int MaxX, int MaxY)
        {
            Random r = new Random();
            return new Vector2(r.Next(0, MaxX), r.Next(0, MaxY));
        }

    }
}