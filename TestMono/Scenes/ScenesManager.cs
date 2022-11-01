using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMono.Scenes;

internal class ScenesManager
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly SpriteBatch _spriteBatch;
    private readonly GameWindow _gameWindow;

    public IScene CurrentScene { get; set; }

    public ScenesManager(
        GraphicsDeviceManager graphics,
        SpriteBatch spriteBatch,
        GameWindow gameWindow)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _gameWindow = gameWindow;
    }

    public void CreateMainScene()
    {
        CurrentScene = new MainScene(_graphics, _spriteBatch);
    }

    public void CreateGameScene()
    {
        CurrentScene = new GameScene(_graphics, _spriteBatch, _gameWindow);
    }

    public void CurrentSceneUpdate(GameTime gameTime) => CurrentScene?.Update(gameTime);

    public void CurrentSceneDraw(GameTime gameTime) => CurrentScene?.Draw(gameTime);
}
