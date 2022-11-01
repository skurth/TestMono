using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestMono.GameObjects;
using TestMono.GameObjects.Map;
using TestMono.GameObjects.Utils;
using TestMono.Network.Client;

namespace TestMono.Scenes;

public class GameScene : IScene
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _font;

    private OrthographicCamera _camera;
    public OrthographicCamera Camera { get => _camera; }
    private const float _CAMERA_MOVEMENT_SPEED = 400;
    private const float _CAMERA_MAX_OUTSIDE_MAP = 100;
    private const float _CAMERA_MOVE_MOUSE_DISTANCE = 100;

    public List<Player> Players { get; set; }

    public Player LocalPlayer { get; set; }

    public Map Map { get; set; }

    private UnitsUtils UnitsUtils { get; set; }

    private ApplicationType AppType { get; init; }

    private ClientGameInstance GameInstance { get; init; }

    public GameScene(
        GraphicsDeviceManager graphics, 
        SpriteBatch spriteBatch,
        GameWindow window,
        Map map,
        List<Player> players,
        Player localPlayer,
        ApplicationType appType,
        ClientGameInstance gameInstance)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _font = Game1.CurrentGame.MainFont;

        var viewportAdapter = new BoxingViewportAdapter(window, _graphics.GraphicsDevice, 1024, 768);
        _camera = new OrthographicCamera(viewportAdapter);

        Map = map;
        Players = players;
        LocalPlayer = localPlayer;
        AppType = appType;
        GameInstance = gameInstance;
        UnitsUtils = new UnitsUtils(this);

        window.Title = appType.ToString();
    }

    public void Update(GameTime gameTime)
    {            
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            var worldPositionRectangle = new RectangleF(worldPosition.X, worldPosition.Y, 1, 1);
            TrySelectPlayerUnit(worldPositionRectangle);
        }
        else if (mouseState.RightButton == ButtonState.Pressed)
        {
            var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            MoveSelectedUnitRequest(worldPosition);
        }            

        UpdateCamera(gameTime, mouseState);

        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.R))
            _camera.ZoomIn(deltaSeconds);

        if (keyboardState.IsKeyDown(Keys.F))
            _camera.ZoomOut(deltaSeconds);

        foreach (var player in Players)
        {
            player.Update(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        var transformMatrix = _camera.GetViewMatrix();

        _spriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);

        Map.Draw(_spriteBatch);

        foreach (var player in Players)
        {
            player.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        _spriteBatch.Begin();
        _spriteBatch.DrawRectangle(new RectangleF(250, 250, 50, 50), Color.Red, 1f); //not dependent on camera
        var diagnostics = GetGameDiagnostics();
        _spriteBatch.DrawString(_font, diagnostics, new Vector2(10, 26), Color.Beige);            
        _spriteBatch.End();
    }

    private void UpdateCamera(GameTime gameTime, MouseState mouseState)
    {
        var cameraDirection = GetMovementDirection(mouseState) * _CAMERA_MOVEMENT_SPEED * gameTime.GetElapsedSeconds();
        if (cameraDirection == Vector2.Zero) { return; }

        if (_camera.Position.X < (-1 * _CAMERA_MAX_OUTSIDE_MAP) && cameraDirection.X < 0) { return; }
        if (_camera.Position.Y < (-1 * _CAMERA_MAX_OUTSIDE_MAP) && cameraDirection.Y < 0) { return; }
        if (_camera.BoundingRectangle.Right > (Map.MaxMapSize.X + _CAMERA_MAX_OUTSIDE_MAP) && cameraDirection.X > 0) { return; }
        if (_camera.BoundingRectangle.Bottom > (Map.MaxMapSize.Y + _CAMERA_MAX_OUTSIDE_MAP) && cameraDirection.Y > 0) { return; }

        _camera.Move(cameraDirection);
    }

    private Vector2 GetMovementDirection(MouseState mouseState)
    {
        var movementDirection = Vector2.Zero;
        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Down) || mouseState.Y > _graphics.PreferredBackBufferHeight - _CAMERA_MOVE_MOUSE_DISTANCE)
        {
            movementDirection += Vector2.UnitY;
        }
        if (state.IsKeyDown(Keys.Up) || mouseState.Y < _CAMERA_MOVE_MOUSE_DISTANCE)
        {
            movementDirection -= Vector2.UnitY;
        }
        if (state.IsKeyDown(Keys.Left) || mouseState.X < _CAMERA_MOVE_MOUSE_DISTANCE)
        {
            movementDirection -= Vector2.UnitX;
        }
        if (state.IsKeyDown(Keys.Right) || mouseState.X > _graphics.PreferredBackBufferWidth - _CAMERA_MOVE_MOUSE_DISTANCE)
        {
            movementDirection += Vector2.UnitX;
        }
        return movementDirection;
    }

    public void TrySelectPlayerUnit(RectangleF mouseClick)
    {
        foreach (var player in Players)
        {
            player.UnselectAllUnits();                
        }

        foreach (var player in Players)
        {
            if (player.TrySelectUnit(mouseClick))
            {
                return;
            }
        }
    }

    public void MoveSelectedUnitRequest(Vector2 endPosition)
    {
        if (!Map.IsInside(endPosition)) 
            return;

        if (LocalPlayer is not null)
        {
            var unit = LocalPlayer.Units.FirstOrDefault(x => x.IsSelected);
            if (unit is null) 
                return;
            
            GameInstance.MoveUnitRequest(unit.Id, endPosition.X - unit.Width / 2, endPosition.Y - unit.Height / 2);            
        }            
    }

    public void MoveUnitOrder(int unitId, float endPositionX, float endPositionY)
    {
        var unit = UnitsUtils.GetUnitById(unitId);
        if (unit == null)
            return;

        unit.EndPosition = new Vector2(endPositionX, endPositionY);
    }

    private string GetGameDiagnostics()
    {
        var sb = new StringBuilder();

        if (AppType == ApplicationType.Server)
        {
            var server = ApplicationManager.Server;
            sb.Append("Server");
            sb.Append(" / ");
            sb.Append($"Players {server.GameInstance.ConnectedPlayersCount}");
        }
        else
        {
            var client = ApplicationManager.Client;
            sb.Append("Client");
            sb.Append(" / ");
            sb.Append($"Id {client.PlayerId}");
        }

        return sb.ToString();
    }
}
