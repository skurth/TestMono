using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;
using TestMono.Helpers;
using TestMono.Network.Client;
using TestMono.Scenes;

namespace TestMono
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;           

        private static Game1 _currentGame;
        public static Game1 CurrentGame { get => _currentGame; }
        public SpriteFont MainFont { get; set; }

        private ScenesManager _scenesManager;
        public ScenesManager ScenesManager { get => _scenesManager; }

        private FpsCounter _fpsCounter;

        public Game1()
        {
            _currentGame = this;
            
            _graphics = new GraphicsDeviceManager(this);            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = true;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(300);
            _graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // TODO: Add your initialization logic here

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();                       
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            MainFont = Content.Load<SpriteFont>("MainFont");

            _scenesManager = new ScenesManager(_graphics, _spriteBatch, Window);
            _scenesManager.CreateMainScene();           

            _fpsCounter = new FpsCounter(this, MainFont, new Vector2(10, 10));

            PlayerUtils.Init();
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ApplicationManager.PollEvents();

            _fpsCounter.Update(gameTime);

            _scenesManager.CurrentSceneUpdate(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _fpsCounter.Draw(gameTime);

            _scenesManager.CurrentSceneDraw(gameTime);

            _spriteBatch.Begin();
            var diagnostics = GetGameDiagnosticString(gameTime);
            _spriteBatch.DrawString(MainFont, diagnostics, new Vector2(100, 10), Color.Fuchsia);
            _spriteBatch.End();

            base.Draw(gameTime);
        }        

        private string GetGameDiagnosticString(GameTime gameTime)
        {
            var sb = new StringBuilder();

            var frameRate = Math.Round(1 / (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
            sb.Append($"FPS: {frameRate}");

            sb.Append(" ");

            if (_scenesManager.CurrentScene is GameScene)
            {
                var gameScene = _scenesManager.CurrentScene as GameScene;
                sb.Append($"Camera X: {gameScene.Camera.Position.X} Camera Y: {gameScene.Camera.Position.Y}");
            }            

            return sb.ToString();
        }

    }
}