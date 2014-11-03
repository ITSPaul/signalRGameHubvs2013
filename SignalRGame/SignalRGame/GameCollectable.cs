using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SignalRGameServer;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SignalRGame
{
    class GameCollectable
    {
        Collectable _collectable;
        bool _visible = false;
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        Rectangle _boundingRect;
        public Rectangle BoundingRect
        {
            get { return _boundingRect; }
            set { _boundingRect = value; }
        }
        public Collectable Collectable
        {
            get { return _collectable; }
            set { _collectable = value; }
        }
        public GameCollectable(Collectable c, Texture2D txToDraw) 
        {
            _collectable = c;
            _boundingRect = new Rectangle((int)c.Position.X, (int)c.Position.Y, txToDraw.Width, txToDraw.Height);
        }

        // must be drawn from within a spritebatch
        public void draw(Texture2D tex, SpriteBatch sp){
            sp.Draw(tex, _boundingRect, Color.White);

        }
    }
}
