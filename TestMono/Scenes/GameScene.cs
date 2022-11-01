using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Collections.Generic;
using TestMono.GameObjects;
using TestMono.GameObjects.Map;

namespace TestMono.Scenes
{
    internal class GameScene : IScene
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly SpriteBatch _spriteBatch;

        private OrthographicCamera _camera;
        public OrthographicCamera Camera { get => _camera; }
        private const float _CAMERA_MOVEMENT_SPEED = 400;
        private const float _CAMERA_MAX_OUTSIDE_MAP = 100;
        private const float _CAMERA_MOVE_MOUSE_DISTANCE = 100;

        public List<Player> Players { get; set; }

        public Player LocalPlayer { get; set; }

        public Map Map { get; set; }

        public GameScene(
            GraphicsDeviceManager graphics, 
            SpriteBatch spriteBatch, 
            GameWindow window)
        {
            _graphics = graphics;
            _spriteBatch = spriteBatch;

            var viewportAdapter = new BoxingViewportAdapter(window, _graphics.GraphicsDevice, 1024, 768);
            _camera = new OrthographicCamera(viewportAdapter);

            Players = new List<Player>();
            LocalPlayer = new Player(true);

            Players.Add(LocalPlayer);
            Players.Add(new Player(false));

            Map = new Map();
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
                MoveSelectedUnit(worldPosition);
            }            

            UpdateCamera(gameTime, mouseState);

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.R))
                _camera.ZoomIn(deltaSeconds);

            if (keyboardState.IsKeyDown(Keys.F))
                _camera.ZoomOut(deltaSeconds);

            //if (keyboardState.IsKeyDown(Keys.C) && !_client.IsRunning)
            //    _client.Connect();

            //if (keyboardState.IsKeyDown(Keys.V) && _client.IsRunning)
            //    _client.Connect();

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

        public void MoveSelectedUnit(Vector2 endPosition)
        {
            if (!Map.IsInside(endPosition)) 
                return;

            LocalPlayer.MoveSelectedUnit(endPosition);
        }
    }
}
