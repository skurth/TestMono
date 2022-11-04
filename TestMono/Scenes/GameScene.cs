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
using TestMono.GameObjects.Buildings;
using TestMono.GameObjects.Map;
using TestMono.GameObjects.Units;
using TestMono.GameObjects.Weapons;
using TestMono.Helpers;
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
    private BuildingsUtils BuildingsUtils { get; set; }

    private ApplicationType AppType { get; init; }

    private ClientGameInstance ClientGameInstance { get; init; }
    private ServerGameInstance ServerGameInstance { get; init; }

    private MouseState _previousMouseState = new MouseState();

    private DateTime? _lastServerTimestampSync = null;
    private TimestampInfo _timestampInfo = null;


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
        BuildingsUtils = new BuildingsUtils(this);

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
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            if (keyboardState.IsKeyDown(Keys.H))
            {
                var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                BuildingSetFoundationRequest(worldPosition);
            }
            else
            {
                var worldPosition = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                var worldPositionRectangle = new RectangleF(worldPosition.X, worldPosition.Y, 1, 1);
                TrySelectPlayerGameObject(worldPositionRectangle);
            }
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
            CheckCollisions(gameTime);
            CheckUnitsNearShootingBuildings();
        }
        else if (AppType == ApplicationType.Client)
        {
            if (!_lastServerTimestampSync.HasValue || DateTime.Now - _lastServerTimestampSync.Value > TimeSpan.FromSeconds(10))
            {
                ClientGameInstance.SyncServerTimestampRequest();
                _lastServerTimestampSync = DateTime.Now;
            }
        }

        UpdateCamera(gameTime, mouseState);

        if (keyboardState.IsKeyDown(Keys.R))
            _camera.ZoomIn((float)gameTime.ElapsedGameTime.TotalSeconds);

        if (keyboardState.IsKeyDown(Keys.F))
            _camera.ZoomOut((float)gameTime.ElapsedGameTime.TotalSeconds);

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
                    ApplyPlayerPacket_MoveUnitRequest((MoveUnitRequestPacket)basePacket);
                    break;
                case PacketType.BuildingSetFoundationRequestPacket:
                    ApplyPlayerPacket_BuildingSetFoundationRequest((BuildingSetFoundationRequestPacket)basePacket);
                    break;
                default:
                    throw new NotImplementedException($"PacketType {playerPacket.Packet.PacketType} not implemented");
            }

            playerPacket.Handled = true;
        }
    }
    private void ApplyPlayerPacket_MoveUnitRequest(MoveUnitRequestPacket packet)
    {
        Debug.WriteLine($"{DateTime.Now} Server ApplyPlayerPacket -> {JsonConvert.SerializeObject(packet)}");

        var orderPacket = new MoveUnitOrderPacket()
        {
            PacketType = (int)PacketType.MoveUnitOrderPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            UnitId = packet.UnitId,
            PositionX = packet.PositionX,
            PositionY = packet.PositionY
        };

        // ToDo Check if ok?        
        ClientGameInstance.MoveUnitOrder(orderPacket);

        // If OK
        ServerGameInstance.Server.SendMoveUnitOrderPacket(orderPacket);
    }
    private void ApplyPlayerPacket_BuildingSetFoundationRequest(BuildingSetFoundationRequestPacket packet)
    {
        Debug.WriteLine($"{DateTime.Now} Server ApplyPlayerPacket -> {JsonConvert.SerializeObject(packet)}");        

        var tmpHouse = new House(-1, LocalPlayer, Vector2.Zero); //Just to get Width and Height
        if (!BuildingsUtils.CanBuildBuilding(packet.PositionX, packet.PositionY, tmpHouse.Width, tmpHouse.Height))
        {
            //Response to requesting client -> Can't be built
            return;
        }

        var orderPacket = new BuildingSetFoundationOrderPacket()
        {
            PacketType = (int)PacketType.BuildingSetFoundationOrderPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            BuildingId = BuildingsUtils.GetNextBuildingId(),
            PlayerId = packet.PlayerId,
            PositionX = packet.PositionX,
            PositionY = packet.PositionY
        };
        
        ClientGameInstance.BuildingSetFoundationOrder(orderPacket);        
        ServerGameInstance.Server.SendBuildingSetFoundationOrderPacket(orderPacket);
    }

    private void CheckCollisions(GameTime gameTime)
    {
        foreach (var player in Players)
        {
            foreach (var unit in player.Units)
            {
                if (unit.EndPosition == Vector2.Zero) 
                    continue;

                IBuilding collisionBuilding = null;
                if (CheckUnitCollision(unit, ref collisionBuilding))
                {
                    // finish building
                    if (collisionBuilding.State == BuildingBuiltState.Foundation && collisionBuilding.Player == unit.Player)
                    {
                        BuildingSetBuiltOrder(collisionBuilding.Id);

                        ServerGameInstance.Server.SendBuildingSetBuiltOrderPacket(new BuildingSetBuiltOrderPacket()
                        {
                            PacketType = (int)PacketType.BuildingSetBuiltOrderPacket,
                            Timestamp = TimeUtils.GetCurrentTimestamp(),
                            BuildingId = collisionBuilding.Id,
                        });
                    }

                    unit.EndPosition = Vector2.Zero;

                    // throw unit outside of building
                    var unitTargetPosition = new Vector2(collisionBuilding.Position.X + collisionBuilding.Width + 1, collisionBuilding.Position.Y + collisionBuilding.Height + 1);

                    MoveUnitStopOrder(unit.Id, unitTargetPosition.X, unitTargetPosition.Y);

                    ServerGameInstance.Server.SendMoveUnitStopOrderPacket(new MoveUnitStopOrderPacket()
                    {
                        PacketType = (int)PacketType.MoveUnitStopOrderPacket,
                        Timestamp = TimeUtils.GetCurrentTimestamp(),
                        UnitId = unit.Id,
                        PositionX = unitTargetPosition.X,
                        PositionY = unitTargetPosition.Y
                    });
                }
            }
        }
    }

    private bool CheckUnitCollision(IUnit unit, ref IBuilding collisionBuilding)
    {
        foreach (var player in Players)
        {
            foreach (var building in player.Buildings)
            {
                if (unit.Rectangle.Intersects(building.Rectangle))
                {
                    collisionBuilding = building;
                    return true;
                }
            }
        }

        return false;
    }

    private bool _enemyUnitIsCloseBuilding = false;
    private void CheckUnitsNearShootingBuildings()
    {
        var allUnits = new List<IUnit>();
        var allBuildings = new List<IBuilding>();

        _enemyUnitIsCloseBuilding = false;

        foreach (var player in Players)
        {
            allUnits.AddRange(player.Units);
            allBuildings.AddRange(player.Buildings);
        }

        foreach (var unit in allUnits)
        {
            foreach (var building in allBuildings)
            {
                if (building is not House)
                    continue;

                if (unit.Player == building.Player)
                    continue;

                var house = (House)building;

                var distance = (unit.CenterPosition - building.CenterPosition).Length();
                if (distance < 300)
                {
                    if (house.CanShotArrow)
                    {
                        HouseShotArrowOrder(house.Id, unit.CenterPosition.X, unit.CenterPosition.Y);
                    }

                    _enemyUnitIsCloseBuilding = true;
                }
            }
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
        var gameDiagnostics = GetGameDiagnostics();
        _spriteBatch.DrawString(_font, gameDiagnostics, new Vector2(10, 26), Color.Beige);
        var timestampDiagnostics = GetTimestampInfoDiagnostics();
        _spriteBatch.DrawString(_font, timestampDiagnostics, new Vector2(700, 10), Color.Beige);

        if (_enemyUnitIsCloseBuilding)
        {
            _spriteBatch.DrawString(_font, "Enemy is close to building", new Vector2(700, 24), Color.Beige);
        }

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

    public void TrySelectPlayerGameObject(RectangleF mouseClick)
    {
        foreach (var player in Players)
        {
            player.UnselectAllUnits();
            player.UnselectAllBuildings();
        }

        foreach (var player in Players)
        {
            if (player.TrySelectUnit(mouseClick) || player.TrySelectBuilding(mouseClick))
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

    public void MoveUnitStopOrder(int unitId, float endPositionX, float endPositionY)
    {
        var unit = UnitsUtils.GetUnitById(unitId);
        if (unit == null)
            return;

        unit.EndPosition = new Vector2(endPositionX, endPositionY);
    }

    public void BuildingSetFoundationRequest(Vector2 endPosition)
    {
        if (!Map.IsInside(endPosition))
            return;

        if (LocalPlayer is not null)
        {
            var tmpHouse = new House(-1, LocalPlayer, Vector2.Zero); //Just to get Width and Height

            ClientGameInstance.BuildingSetFoundationRequest(LocalPlayer.Id, endPosition.X - tmpHouse.Width / 2, endPosition.Y - tmpHouse.Height / 2);
        }
    }
    public void BuildingSetFoundationOrder(int buildingId, string playerId, float endPositionX, float endPositionY)
    {
        var player = Players.FirstOrDefault(x => x.Id.Equals(playerId, StringComparison.OrdinalIgnoreCase));
        if (player is null)
            return;

        var buildingPosition = new Vector2(endPositionX, endPositionY);
        var building = new House(buildingId, player, buildingPosition);
        player.Buildings.Add(building);

        if (LocalPlayer is not null)
        {
            MoveSelectedUnitRequest(building.CenterPosition);
        }
    }

    public void BuildingSetBuiltOrder(int buildingId)
    {
        var building = BuildingsUtils.GetBuildingById(buildingId);
        if (building is null)
            return;

        building.State = BuildingBuiltState.Built;
    }

    public void HouseShotArrowOrder(int buildingId, float endPositionX, float endPositionY)
    {
        var building = BuildingsUtils.GetBuildingById(buildingId);
        if (building is null)
            return;

        var house = (House)building;

        house.Arrows.Add(new Arrow(house, building.CenterPosition, new Vector2(endPositionX, endPositionY)));
    }

    public void SetTimestampInfo(TimestampInfo timestampInfo)
    {
        _timestampInfo = timestampInfo;
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
    private string GetTimestampInfoDiagnostics()
    {
        var sb = new StringBuilder();

        if (AppType == ApplicationType.Server)
        {

        }
        else
        {
            if (_timestampInfo is null)
                return String.Empty;

            sb.AppendLine($"Server Time: {_timestampInfo.ServerTime.ToString("HH:mm:ss.ffffff")}");
            sb.AppendLine($"Latency: {_timestampInfo.Latency}ms");
            sb.AppendLine($"Server Delta: {_timestampInfo.ServerDelta}ms");
            sb.AppendLine($"Time Delta: {_timestampInfo.TimeDelta}ms");
        }

        return sb.ToString();
    }
}

public class TimestampInfo
{
    public long ServerTimestamp { get; set; }
    public DateTime ServerTime { get => TimeUtils.GetDateTimeFromTimestamp(ServerTimestamp); }
    public int Latency { get; set; }
    public int ServerDelta { get; set; }
    public int TimeDelta { get; set; }
}