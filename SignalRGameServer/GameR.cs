using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRGameServer
{
    public class Player
    {
        int _playerID;

        public int PlayerID
        {
            get { return _playerID; }
            set { _playerID = value; }
        }
        Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        int _score;

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public Player(int id, Vector2 pos)
        {
            _playerID = id;
            _position = pos;
        }
    }
}