using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMono.Network.Client;

namespace TestMono.Scenes;

public class ScenesManager
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

    public void CreateGameScene(CreateGameSceneInfo sceneInfo, ClientGameInstance clientGameInstance)
    {
        CurrentScene = new GameScene(
            _graphics, 
            _spriteBatch, 
            _gameWindow,
            sceneInfo.Map,
            sceneInfo.Players,
            sceneInfo.LocalPlayer,
            sceneInfo.AppType,
            clientGameInstance);
    }

    public void CurrentSceneUpdate(GameTime gameTime) => CurrentScene?.Update(gameTime);

    public void CurrentSceneDraw(GameTime gameTime) => CurrentScene?.Draw(gameTime);
}
