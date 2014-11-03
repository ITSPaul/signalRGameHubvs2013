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

        public void joinServer()
        {
            if (!_gameStarted)
            {
                //_playerCounter++;
                int count = _players.Count;
                _players.Add(count, new Player(count, Vector2.Zero));
                Clients.Caller.JoinedServer(_players[count]);
                Clients.Others.AddPlayer(_players[count]);
                _playersPlaying++;
                if (_playersPlaying > 1)
                {
                    _startTime = new Timer(4000);
                    _startTime.Elapsed += _startTime_Elapsed;
                    _startTime.Start();
                    Clients.All.TimerOn(_startTime.Interval);
                }
            }
        }

        void _startTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            Clients.All.StartGame();
            Clients.All.Accept(createCollectables());
            _gameStarted = true;
        }

        private Dictionary<int,Collectable> createCollectables()
        {
            Random r = new Random();
            int collectableCount = r.Next(3, 10);
            int[] possibles = { 10, 20, 30 };
            for (int i = 0; i < collectableCount; i++)
            {
                // Add a collectable to the collection with a random position and a random possible value
                _collectables.Add(i,
                    new Collectable(i, new Vector2(r.Next(40, 1000), r.Next(40, 700)),
                        possibles[r.Next(0, 2)]
                    ));
            }
            return _collectables;
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
            }
        }

        public void movePlayer(Player p)
        {
                _players[p.PlayerID].Position = p.Position;
                Clients.Others.UpdateOtherPlayerPosition(p);
        }


    }
}