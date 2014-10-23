using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.AspNet.SignalR.Client;
using SignalRGameServer;

namespace SignalRGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private HubConnection connection;
        private IHubProxy proxy;
        GamePlayer gamePlayer;
        SpriteFont GameMessages;
        bool started = false;
        List<string> _debugMessages = new List<string>();
        bool debugOn = false;

        SpriteFont debug;
        
        private string _hubMessage = "";
        private float speed = 5.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            spriteBatch = new SpriteBatch(GraphicsDevice);
            connection = new HubConnection("http://localhost:49669/");
            proxy = connection.CreateHubProxy("GameHub");
                
            try
            {
                connection.Start().Wait();
                proxy.Invoke("JoinServer", new object[] {});
                _hubMessage = "Joining Game Server";
            }

            catch (System.Exception ex)
            {
                _hubMessage = ex.InnerException.Message;
                this.Exit();
            }


            // Messages expected from the server
            Action<Player> positions = UpdatePositionOtherPlayer;
            Action<Player> joined = JoinedXNAGame;
            Action<Player> deleteOtherPlayer = deletePlayerfromOthers;
            Action<Player> AddOtherPlayer = AddToOtherPlayers;
            Action start = startXNA;
            
            proxy.On("StartGame", start);
            proxy.On("UpdateOtherPlayerPosition", positions);
            proxy.On("JoinedServer", joined);
            proxy.On("DeleteClientPlayerFromOthers", deleteOtherPlayer);
            proxy.On("AddPlayer", AddOtherPlayer);

            base.Initialize();
        }

        private void startXNA()
        {
            _hubMessage = "And We are off. Collect as many collectables as you can";
            started = true;
        }

        private void JoinedXNAGame(Player p)
        {
            //Player p = new Player(id, getrandomPos());
            gamePlayer = new GamePlayer(Content.Load<SpriteFont>("message"),
                Content.Load<Texture2D>("ship_lvl8"), p);
            //proxy.Invoke("NewPlayer", new object[] { p });
            move(getrandomPos());
            _hubMessage = " Player " + gamePlayer.Player.PlayerID.ToString() + " Joined Game Server ";

        }


        private void AddToOtherPlayers(Player other)
        {
            if(gamePlayer != null)
                gamePlayer.OtherPlayers.Add(other.PlayerID,
                    new GamePlayer(Content.Load<SpriteFont>("message"), 
                        Content.Load<Texture2D>("ship_lvl8"), other));
            _hubMessage = "Player " + other.PlayerID.ToString() + " added to this client ";
        }

        private void deletePlayerfromOthers(Player other)
        {
            if (gamePlayer != null)
                gamePlayer.OtherPlayers.Remove(other.PlayerID);
            _hubMessage = "Player " + other.PlayerID.ToString() + " Removed from this client";
        }

        
        private void UpdatePositionOtherPlayer(Player p)
        {
            gamePlayer.OtherPlayers[p.PlayerID].Player.Position = p.Position;
        }

        public void move(Vector2 Delta)
        {
            gamePlayer.Move(Delta);
            _hubMessage = "Moving Player " + gamePlayer.Player.PlayerID.ToString() + " to " + gamePlayer.Player.Position.ToString();
            proxy.Invoke("movePlayer", new object[] { this.gamePlayer.Player });
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameMessages = Content.Load<SpriteFont>("message");
            debug = Content.Load<SpriteFont>("debugHud");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if(gamePlayer != null)
                proxy.Invoke("DeletePlayer", new object[] { this.gamePlayer.Player});
            base.OnExiting(sender, args);
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            if (Keyboard.GetState().IsKeyUp(Keys.D))
                debugOn = !debugOn;
            if (Keyboard.GetState().IsKeyUp(Keys.R))
                _debugMessages.Clear();

            if(started)
            { 
            if(gamePlayer == null && Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    move(new Vector2(-1, 0) * speed);
                }
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            if(gamePlayer != null)
                gamePlayer.Draw(spriteBatch);
            spriteBatch.DrawString(GameMessages, _hubMessage, new Vector2(50,50), Color.White);
            if (debugOn)
                drawHud();
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void drawHud()
        {
            Vector2 startPos = new Vector2(10,10);
            foreach (string s in _debugMessages)
            {
                spriteBatch.DrawString(debug, s, startPos,Color.Green);
                startPos += new Vector2(0, 10);
            }
        }

        private Vector2 getrandomPos()
        {
            Random r = new Random();
            float x = r.Next(100, GraphicsDevice.Viewport.Bounds.Width - 100);
            float y = r.Next(100, GraphicsDevice.Viewport.Bounds.Height - 100);
            return new Vector2(x,y);
        }

    }
}
