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
using System.Threading.Tasks;

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
        Dictionary<COLLECTABLE_TYPE, Texture2D> collectableTextures = new Dictionary<COLLECTABLE_TYPE,Texture2D>();
        Dictionary<int, GameCollectable> _gameCollectables = new Dictionary<int, GameCollectable>();
        List<string> debugMessages = new List<string>();
        private string _hubMessage = "";
        private float speed = 5.0f;
        private double _countDown;
        SpriteFont debug;
        bool debugOn = true;
        GAMESTATE hubState;
        List<string> scoreBoard;

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
            
            
            // Messages expected from the server
            Action<Player> positions = UpdatePositionOtherPlayer;
            Action<Player> joined = JoinedXNAGame;
            Action<Player> deleteOtherPlayer = deletePlayerfromOthers;
            Action<Player> AddOtherPlayer = AddToOtherPlayers;
            Action<double> TimerOn = TimeOn;
            Action<Dictionary<int, Collectable>> Accept = acceptCollectables;
            Action start = startXNA;
            Action<Collectable> CollectableIsGone = Gone;
            Action<List<KeyValuePair<int,Player>>> presentScores = PresentScores;
            Action quitGame = quitServer;
            proxy.On("StartGame", start);
            proxy.On("UpdateOtherPlayerPosition", positions);
            proxy.On("JoinedServer", joined);
            proxy.On("DeleteClientPlayerFromOthers", deleteOtherPlayer);
            proxy.On("AddOtherPlayer", AddOtherPlayer);
            proxy.On("TimerOn", TimerOn);
            proxy.On("Accept", Accept);
            proxy.On("Gone", CollectableIsGone);
            proxy.On("PresentScoreBoard", presentScores);
            proxy.On("quitGame", quitGame);
            base.Initialize();
        }

        private void quitServer()
        {
            this.Exit();
        }

        private void PresentScores(List<KeyValuePair<int,Player>> ScoreDictionary)
        {
            scoreBoard = new List<string>();
            foreach (KeyValuePair<int, Player> item in ScoreDictionary)
                scoreBoard.Add("Player " + item.Value.PlayerID.ToString() + " score " 
                                    + item.Value.Score.ToString());
        }


        private void acceptCollectables(Dictionary<int,Collectable> delivered)
        {
            //debugMessages.Add("Collectables delivered " + delivered.Count().ToString());
            // Add the delivered collectable as components to collectable game objects
         if(_gameCollectables.Count == 0)   
            foreach (KeyValuePair<int, Collectable> c in delivered)
                _gameCollectables.Add(c.Value.Id, new GameCollectable(c.Value,
                    collectableTextures[c.Value.Type]));
        }

        private void TimeOn(double countDown)
        {
            _countDown = countDown;
        }

        private void startXNA()
        {
            _hubMessage = "And We are off. Collect as many collectables as you can";
            started = true;
            foreach (KeyValuePair<int,GameCollectable> g in _gameCollectables)
                g.Value.Visible = true;
        }
        // Phasing this out in favour of returning a player when joining
        private void JoinedXNAGame(Player p)
        {
            //proxy.Invoke<GAMESTATE>("getCurrentGameState").ContinueWith((coninuation)
            //=> { debugMessages.Add( " Game State is" + coninuation.Result.ToString()); });

            //proxy.Invoke<Player>("getPlayer", new object[] {p.PlayerID})
            //    .ContinueWith((coninuation)
            //=> { debugMessages.Add(" player returned is " + coninuation.Result.PlayerID.ToString()); });

            gamePlayer = new GamePlayer(Content.Load<SpriteFont>("message"),
                Content.Load<Texture2D>("ship_lvl8"), p);
            _hubMessage = " Player " + gamePlayer.Player.PlayerID.ToString() + " Joined Game Server ";
        }

        private void AddToOtherPlayers(Player other)
        {
            
            if (gamePlayer != null)
            {
                gamePlayer.OtherPlayers.Add(other.PlayerID,
                    new GamePlayer(Content.Load<SpriteFont>("message"),
                        Content.Load<Texture2D>("ship_lvl8"), other));
                debugMessages.Add("Player " + other.PlayerID.ToString() + " added to this client ");
            }
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
            debug = Content.Load<SpriteFont>("debug");
            // TODO: use this.Content to load your game content here
            collectableTextures.Add(COLLECTABLE_TYPE.STANDARD, Content.Load<Texture2D>("SIMPLE"));
            collectableTextures.Add(COLLECTABLE_TYPE.MEDIUM, Content.Load<Texture2D>("MEDIUM"));
            collectableTextures.Add(COLLECTABLE_TYPE.COMPLEX, Content.Load<Texture2D>("COMPLEX"));


            try
            {
                connection.Start().ContinueWith((started) =>
                {
                    proxy.Invoke<Vector2>("WorldsEnd").ContinueWith((dimensions)
                       => { 
                            graphics.PreferredBackBufferWidth = (int)dimensions.Result.X;
                             graphics.PreferredBackBufferHeight = (int)dimensions.Result.Y;
                    });

                    proxy.Invoke<Player>("JoinServer").ContinueWith((OnCreated)
                       =>
                    {
                        gamePlayer = new GamePlayer(Content.Load<SpriteFont>("message"),
                          Content.Load<Texture2D>("ship_lvl8"), OnCreated.Result);
                        _hubMessage = " Player " + gamePlayer.Player.PlayerID.ToString();
                        _hubMessage += " Joined Game Server ";

                        proxy.Invoke<List<KeyValuePair<int, Player>>>("getOthers", new object[] { gamePlayer.Player.PlayerID })
                            .ContinueWith((o) =>
                                {
                                    List<KeyValuePair<int, Player>> players = o.Result;
                                    if (o.Result.Count() > 0)
                                    {
                                        foreach (KeyValuePair<int, Player> item in players)
                                        {
                                            gamePlayer.OtherPlayers.Add(item.Value.PlayerID,
                                                new GamePlayer(Content.Load<SpriteFont>("message"),
                                                    Content.Load<Texture2D>("ship_lvl8"), item.Value));
                                        }

                                    }
                                });
                            

                    });
                });
                //                _hubMessage = "Joining Game Server";
            }


            catch (System.Exception ex)
            {
                _hubMessage = ex.InnerException.Message;
                this.Exit();
            }


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
            proxy.Invoke<GAMESTATE>("getCurrentGameState").ContinueWith((exitingServer) =>
            {
                if(exitingServer.Result != GAMESTATE.FINISHED)
                    if (gamePlayer != null)
                        proxy.Invoke("DeletePlayer", new object[] { this.gamePlayer.Player });
                base.OnExiting(sender, args);
            });
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    this.Exit();
            
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                debugOn = !debugOn;

            if (!started)
                _countDown -= gameTime.ElapsedGameTime.Milliseconds;

            if (debugOn && Keyboard.GetState().IsKeyDown(Keys.C))
                debugMessages.Clear();

            if(started)
            {
                _countDown = 0;
                if(gamePlayer != null && Keyboard.GetState().IsKeyDown(Keys.A))
                    {
                        move(new Vector2(-1, 0) * speed);
                    }
                if (gamePlayer != null && Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    move(new Vector2(1, 0) * speed);
                }
                if (gamePlayer != null && Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    move(new Vector2(0, -1) * speed);
                }
                if (gamePlayer != null && Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    move(new Vector2(0, 1) * speed);
                }
                foreach(KeyValuePair<int,GameCollectable> g in  _gameCollectables)
                    if(g.Value.Visible)
                        if (gamePlayer.BoundingRect.Intersects(g.Value.BoundingRect))
                        {
                            gamePlayer.Player.Score += g.Value.Collectable.Value;
                            g.Value.Visible = false;
                            // Inform other clients that this has gone.
                            ShowServerState();
                            proxy.Invoke("Collected", new object[] {gamePlayer.Player, g.Value.Collectable})
                                .ContinueWith((showCollectables) => { serverActiveCollectablesCount(); });
                            //ShowServerState();
                        }
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        public void Gone(Collectable c)
        {
            _gameCollectables[c.Id].Visible = false;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                spriteBatch.Begin();
                if (scoreBoard == null)
                {
                    if (_countDown > 0)
                        spriteBatch.DrawString(GameMessages, "Game Starting in " + ((int)_countDown / 1000).ToString(), new Vector2(400, 20), Color.White);
                    // Draw the player
                    if (gamePlayer != null)
                        gamePlayer.Draw(spriteBatch);
                    else _hubMessage = "Waiting to Join.......";
                    // Draw collectables if delivered
                    if (_gameCollectables.Count > 0)
                        foreach (KeyValuePair<int, GameCollectable> item in _gameCollectables)
                            if (item.Value.Visible)
                                item.Value.draw(collectableTextures[item.Value.Collectable.Type],
                                    spriteBatch);
                    spriteBatch.DrawString(GameMessages, _hubMessage, new Vector2(10, 250), Color.White);
                }
                else drawScoreBoard();

                // debug hud
                if (debugOn)
                    DrawDebugHud(spriteBatch);
                spriteBatch.End();
                // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        private void drawScoreBoard()
        {
            Vector2 pos = new Vector2(100,100);
            foreach (string entry in scoreBoard)
            {
                spriteBatch.DrawString(GameMessages, entry, pos, Color.White);
                pos += new Vector2(0, 20);
            }
        }

        private void DrawDebugHud(SpriteBatch sp)
        {
            Vector2 pos = new Vector2(10,10);
            foreach (string item in debugMessages) 
            {
                sp.DrawString(debug, item, pos, Color.Red);
                pos += new Vector2(0, 20);
            }
        }


        public GAMESTATE ShowServerState()
        {
            GAMESTATE serverState = GAMESTATE.NONE;
            proxy.Invoke<GAMESTATE>("getCurrentGameState").ContinueWith((coninuation)
            => { debugMessages.Add(" Game State is" + coninuation.Result.ToString());
                    serverState = coninuation.Result;
            });
            return serverState;
        }

        public int serverActiveCollectablesCount()
        {
            int count = 0;
            proxy.Invoke<int>("ActiveCollectableCount").ContinueWith((continuation)
            =>
            {
                count = continuation.Result;
                debugMessages.Add(" Active Collectables on Server" + continuation.Result.ToString());
            });
            return count;
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
