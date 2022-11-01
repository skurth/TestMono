using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
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

            if (KeyboardUtils.HasBeenPressed(Keys.T))
                ApplicationManager.SendTest();

            if (KeyboardUtils.HasBeenPressed(Keys.P))
                ApplicationManager.SendTestPacket();

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
                    _spriteBatch.DrawString(_font, "I AM SERVER", new Vector2(300, 200), Color.MediumTurquoise);
                    var serverMessages = ApplicationManager.Server.GetLatestMessagesReceived();
                    _spriteBatch.DrawString(_font, serverMessages, new Vector2(300, 300), Color.MediumTurquoise);
                    break;
                case ApplicationType.Client:
                    var clientMessages = ApplicationManager.Client.GetLatestMessagesReceived();
                    _spriteBatch.DrawString(_font, clientMessages, new Vector2(300, 200), Color.MediumTurquoise);
                    break;
                default:
                    break;
            }
            
            _spriteBatch.End();            
        }
    }
}
