using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SignalRGameServer
{
    public class gSprite
    {
        public Texture2D Image;
        public Vector2 Position;
        public Rectangle BoundingRect;
        public bool visible = true;
        Player _player;

        public Player Player
        {
            get { return _player; }
            set { _player = value; }
        }

        public gSprite(Texture2D spriteImage,
                            Vector2 startPosition, Player p)
        {
            Image = spriteImage;
            Position = startPosition;
            _player = p;
            BoundingRect = new Rectangle((int)startPosition.X, (int)startPosition.Y, Image.Width, Image.Height);

        }

        public void draw(SpriteBatch sp)
        {
            if(visible)
            sp.Draw(Image, Position, Color.White);
        }

        public void Move(Vector2 delta )
        {
            Position += delta;
            BoundingRect.X = (int)Position.X;
            BoundingRect.Y = (int)Position.Y;
        }
    }
}
