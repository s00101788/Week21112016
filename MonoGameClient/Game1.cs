using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Cameras;
using Sprites;
using Microsoft.Xna.Framework.Audio;
using GameData;

namespace MonoGameClient
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        HubConnection serverConnection;
        IHubProxy proxy;
        Vector2 worldCoords;
        SpriteFont messageFont;
        Texture2D backGround;
        private string connectionMessage;
        private bool Connected;
        private Rectangle worldRect;
        private FollowCamera followCamera;
        private bool Joined;

        Player player;
        private string errorMessage;

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
            serverConnection = new HubConnection("http://localhost:3566/");
            serverConnection.StateChanged += ServerConnection_StateChanged;
            proxy = serverConnection.CreateHubProxy("GameHub");
            connectionMessage = string.Empty;
            serverConnection.Start();
            base.Initialize();
        }

        private void ServerConnection_StateChanged(StateChange State)
        {
            switch (State.NewState)
            {
                case ConnectionState.Connected:
                    connectionMessage = "Connected......";
                    Connected = true;
                    startGame();
                    break;
                case ConnectionState.Disconnected:
                    connectionMessage = "Disconnected.....";
                    if (State.OldState == ConnectionState.Connected)
                        connectionMessage = "Lost Connection....";
                    Connected = false;
                    break;
                case ConnectionState.Connecting:
                    connectionMessage = "Connecting.....";
                    Connected = false;
                    break;
            }
        }

        private void startGame()
        {

            Action<int, int> joined = cJoined;
            proxy.On("joined", joined);

            Action<PlayerData> recievePlayer = clientRecievePlayer;
            proxy.On("recievePlayer", recievePlayer);

            Action<string> errmess = recieveError;
            proxy.On("error", errmess);

            proxy.Invoke("join");
                        
        }

        private void recieveError(string message)
        {
            errorMessage = message;
        }

        private void clientRecievePlayer(PlayerData playerData)
        {
            if(player != null)
            {
                player.PlayerInfo = playerData;
            }
        }

        private void cJoined(int worldX, int roldY)
        {
            worldCoords = new Vector2(worldX, roldY);
            // Setup Camera
            worldRect = new Rectangle(new Point(0, 0), worldCoords.ToPoint());
            followCamera = new FollowCamera(this, Vector2.Zero, worldCoords);
            Joined = true;
            // Setup Player
            SetupPlayer();
            proxy.Invoke("getPlayer", new object[] { "Sarah", "Treanor" });

        }

        private void SetupPlayer()
        {
            #region Player Setup

            Texture2D[] txs = new Texture2D[5];
            SoundEffect[] sounds = new SoundEffect[5];
            txs[(int)Player.DIRECTION.LEFT] = Content.Load<Texture2D>(@"Textures\right");
            txs[(int)Player.DIRECTION.RIGHT] = Content.Load<Texture2D>(@"Textures\right");
            txs[(int)Player.DIRECTION.UP] = Content.Load<Texture2D>(@"Textures\up");
            txs[(int)Player.DIRECTION.DOWN] = Content.Load<Texture2D>(@"Textures\down");
            txs[(int)Player.DIRECTION.STANDING] = Content.Load<Texture2D>(@"Textures\stand");


            for (int i = 0; i < sounds.Length; i++)
            {
                sounds[i] = Content.Load<SoundEffect>(@"Audio\PlayerDirection\" + i.ToString());
            }


            player = new Player(txs, sounds, new Vector2(0, 0), 8, 0, 5);
            player.Position = player.PreviousPosition = new Vector2(GraphicsDevice.Viewport.Width / 2 - player.SpriteWidth / 2,
                                          GraphicsDevice.Viewport.Height / 2 - player.SpriteHeight / 2);

            //PrevPlayerPosition = player.Position;

            #endregion Player Setup

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            messageFont = Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            backGround = Content.Load<Texture2D>(@"Textures\background");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!Connected && !Joined) return;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (player != null)
            {
                player.Update(gameTime);
                player.Position = Vector2.Clamp(player.Position, 
                    Vector2.Zero, 
                    (worldCoords - new Vector2(player.SpriteWidth, player.SpriteHeight)));
                followCamera.Follow(player);
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
            if (Connected && Joined)
            {
                DrawPlay();
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(messageFont, connectionMessage,
                                new Vector2(20, 20), Color.White);
                spriteBatch.End();
            }
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void DrawPlay()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, followCamera.CameraTransform);
            spriteBatch.Draw(backGround, worldRect, Color.White);
            if(player != null)
                player.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
