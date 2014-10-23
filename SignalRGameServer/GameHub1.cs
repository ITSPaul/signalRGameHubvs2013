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

        public void joinServer()
        {
            
            //_playerCounter++;
            int count = _players.Count;
            _players.Add(count,new Player(count,Vector2.Zero));
            Clients.Caller.JoinedServer(_players[count]);
            Clients.Others.AddPlayer(_players[count]);
            _playersPlaying++;
            if(_playersPlaying > 1)
            {
                _startTime = new Timer(4000);
                _startTime.Elapsed += _startTime_Elapsed;
                _startTime.Start();
            }
        }

        void _startTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            Clients.All.StartGame();
            Random r = new Random();
            int count = r.Next(5, 10);
            for (int i = 0; i < count; i++)
            {
                _collectables.Add(i, new Collectable(i, new Vector2(r.Next(10, 700), r.Next(10, 400)), r.Next(20)));
            }
            Clients.All.GetCollectables(_collectables);
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
        }

        public void movePlayer(Player p)
        {
                _players[p.PlayerID].Position = p.Position;
                Clients.Others.UpdateOtherPlayerPosition(p);
        }


    }
}