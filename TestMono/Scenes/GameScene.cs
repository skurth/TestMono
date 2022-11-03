using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TestMono.GameObjects;
using TestMono.GameObjects.Map;
using TestMono.GameObjects.Utils;
using TestMono.Network.Client;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;
using TestMono.Network.Server;

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

    private ClientGameInstance ClientGameInstance { get; init; }
    private ServerGameInstance ServerGameInstance { get; init; }

    private MouseState _previousMouseState = new MouseState();

    public GameScene(
        GraphicsDeviceManager graphics, 
        SpriteBatch spriteBatch,
        GameWindow window,
        Map map,
        List<Player> players,
        Player localPlayer,
        ApplicationType appType,
        ClientGameInstance clientGameInstance,
        ServerGameInstance serverGameInstance)
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
        ClientGameInstance = clientGameInstance;
        ServerGameInstance = serverGameInstance;
        UnitsUtils = new UnitsUtils(this);

        window.Title = appType.ToString();

        if (AppType == ApplicationType.Server)
        {
            Game1.CurrentGame.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 10));            
            //Game1.CurrentGame.MaxElapsedTime = TimeSpan.FromMilliseconds(500);
            //Game1.CurrentGame.TargetElapsedTime = TimeSpan.FromMilliseconds(500);
        }
        else if (AppType == ApplicationType.Client)
        {
            if (LocalPlayer?.Units.Count > 0)
            {
                var lookAtPosition = LocalPlayer.Units[0].Position;
                _camera.LookAt(lookAtPosition);
            }            
        }
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();        
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            var worldPositionRectangle = new RectangleF(worldPosition.X, worldPosition.Y, 1, 1);
            TrySelectPlayerUnit(worldPositionRectangle);
        }
        else if (mouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
        {
            var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            MoveSelectedUnitRequest(worldPosition);
        }

        if (AppType == ApplicationType.Server)
        {
            //System.Threading.Thread.Sleep(250);
            ApplyPlayerPackets(gameTime);
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

        _previousMouseState = mouseState;
    }

    private void ApplyPlayerPackets(GameTime gameTime)
    {
        var packetsUnhandledAndOrdered = ServerGameInstance.PlayerPackets.Where(x => !x.Handled).OrderBy(x => x.Packet.Timestamp).ToList();        

        foreach (var playerPacket in packetsUnhandledAndOrdered)
        {
            var basePacket = playerPacket.Packet;            

            switch ((PacketType)basePacket.PacketType)
            {
                case PacketType.MoveUnitRequestPacket:
                    ApplyPlayerPacketMoveUnitOrder((MoveUnitRequestPacket)basePacket);
                    break;
                default:
                    throw new NotImplementedException($"PacketType {playerPacket.Packet.PacketType} not implemented");                    
            }

            playerPacket.Handled = true;
        }        
    }

    private void ApplyPlayerPacketMoveUnitOrder(MoveUnitRequestPacket packet)
    {
        Debug.WriteLine($"{DateTime.Now} Server ApplyPlayerPacket -> {JsonConvert.SerializeObject(packet)}");

        var orderPacket = new MoveUnitOrderPacket()
        {
            PacketType = (int)PacketType.MoveUnitOrderPacket,
            Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds(),
            UnitId = packet.UnitId,
            PositionX = packet.PositionX,
            PositionY = packet.PositionY
        };

        // ToDo Check if ok?
        ClientGameInstance.MoveUnitOrder(orderPacket);

        // If OK
        ServerGameInstance.Server.SendMoveUnitOrderPacket(orderPacket);
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
            
            ClientGameInstance.MoveUnitRequest(unit.Id, endPosition.X - unit.Width / 2, endPosition.Y - unit.Height / 2);            
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
