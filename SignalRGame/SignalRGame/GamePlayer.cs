using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalRGameServer;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.AspNet.SignalR.Client;

namespace SignalRGame
{
    class GamePlayer
    {
        SignalRGameServer.Player _player;

        public SignalRGameServer.Player Player
        {
            get { return _player; }
            set { _player = value; }
        }
        Texture2D _tex;

        public Texture2D Tex
        {
            get { return _tex; }
            set { _tex = value; }
        }
        Rectangle _boundingRect;

        public Rectangle BoundingRect
        {
          get { return _boundingRect; }
          set { _boundingRect = value; }
        }
        // Player is responsible for drawing other players as well as it's self
        Dictionary<int,GamePlayer> _otherPlayers = new Dictionary<int,GamePlayer>();

        public Dictionary<int,GamePlayer> OtherPlayers
        {
            get { return _otherPlayers; }
            set { _otherPlayers = value; }
        }
        
        bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        SpriteFont font;

        public GamePlayer(SpriteFont f, Texture2D tex, Player player)
        {
            _tex = tex;
            _player = player;
            font = f;
            BoundingRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, 
                                            tex.Width, tex.Height);
        }

        public void Move(Vector2 delta)
        {
            _player.Position += delta;
            _boundingRect.X = (int)_player.Position.X;
            _boundingRect.Y = (int)_player.Position.Y;
        }

        public void Draw(SpriteBatch sp)
        {
            sp.Draw(_tex, Player.Position, Color.White);
            sp.DrawString(font, Player.PlayerID.ToString(), Player.Position + new Vector2(0, -50), Color.White);
            foreach(KeyValuePair<int,GamePlayer> item in OtherPlayers)
            {
                GamePlayer gPlayer = item.Value;
                sp.Draw(gPlayer.Tex, gPlayer.Player.Position, Color.Red);
                sp.DrawString(font, gPlayer.Player.PlayerID.ToString(), 
                    gPlayer.Player.Position + new Vector2(0, -50), Color.Red);
            }

        }
    }
}
