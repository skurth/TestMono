using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMono.Helpers;

namespace TestMono.Scenes
{
    internal class MainScene : IScene
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly SpriteBatch _spriteBatch;

        private readonly SpriteFont _font;

        public MainScene(
            GraphicsDeviceManager graphics,
            SpriteBatch spriteBatch)
        {
            _graphics = graphics;
            _spriteBatch = spriteBatch;

            _font = Game1.CurrentGame.MainFont;
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            KeyboardUtils.GetState();

            if (ApplicationManager.AppType == ApplicationType.None && !ApplicationManager.IsRunning)
            {
                if (keyboardState.IsKeyDown(Keys.C))
                    ApplicationManager.OnClientStartRequested();
                if (keyboardState.IsKeyDown(Keys.S))
                    ApplicationManager.OnServerStartRequested();
            }

            if (ApplicationManager.AppType == ApplicationType.Server)
            {
                if (KeyboardUtils.HasBeenPressed(Keys.Q))
                {
                    var gameInstance = ApplicationManager.Server.GameInstance;
                    if (gameInstance.CanStartGame)
                    {
                        gameInstance.InitGame();
                    }
                }                    
            }
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            switch (ApplicationManager.AppType)
            {
                case ApplicationType.None:
                    _spriteBatch.DrawString(_font, "Press [S] for Server, Press [C] for Client", new Vector2(300, 200), Color.MediumTurquoise);
                    break;
                case ApplicationType.Server:
                    var server = ApplicationManager.Server;
                    _spriteBatch.DrawString(_font, "I AM SERVER", new Vector2(300, 200), Color.MediumTurquoise);
                    var serverMessages = server.GetLatestMessagesReceived();
                    _spriteBatch.DrawString(_font, serverMessages, new Vector2(300, 500), Color.MediumTurquoise);

                    _spriteBatch.DrawString(_font, $"{server.GameInstance.ConnectedPlayersCount} Players connected", new Vector2(400, 300), Color.LightGreen);
                    if (server.GameInstance.CanStartGame)
                    {
                        _spriteBatch.DrawString(_font, "Press [Q] to start the game", new Vector2(450, 200), Color.MediumTurquoise);
                    }

                    break;
                case ApplicationType.Client:
                    var client = ApplicationManager.Client;
                    _spriteBatch.DrawString(_font, $"Client {client.PlayerId}", new Vector2(300, 200), Color.MediumTurquoise);
                    var clientMessages = client.GetLatestMessagesReceived();
                    _spriteBatch.DrawString(_font, clientMessages, new Vector2(300, 500), Color.MediumTurquoise);
                    break;
                default:
                    break;
            }

            _spriteBatch.End();
        }
    }
}
