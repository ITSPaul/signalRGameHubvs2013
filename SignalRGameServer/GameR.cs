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

    public class Collectable
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private Vector2 _pos;

        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }
        private int _score;

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public Collectable(int id, Vector2 pos, int score)
        {
            _id = id;
            _pos = pos;
            _score = score;

        }
    }
}