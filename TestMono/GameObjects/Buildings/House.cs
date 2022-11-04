using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using TestMono.GameObjects.Weapons;

namespace TestMono.GameObjects.Units;

public class House : IBuilding
{
    //public Texture2D Texture { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 Position { get; set; }
    public Player Player { get; set; }
    public bool IsSelected { get; set; }

    public Vector2 CenterPosition { get => new Vector2(Position.X + Width / 2, Position.Y + Height / 2); }
    public RectangleF Rectangle { get => new RectangleF(Position.X, Position.Y, Width, Height); }

    public BuildingBuiltState State { get; set; } = BuildingBuiltState.Foundation;    

    public Color CircleColor { get; set; }

    private const int _ARROW_SHOT_RATE_MS = 2000;
    private DateTime? _lastArrowFired = null;
    public List<Arrow> Arrows { get; set; }
    public bool CanShotArrow {
        get => !_lastArrowFired.HasValue || (DateTime.Now - _lastArrowFired.Value).TotalMilliseconds > _ARROW_SHOT_RATE_MS; }

    private readonly SpriteFont _font;

    public House(int id, Player player, Vector2 position)
    {
        //Texture = Game1.CurrentGame.Content.Load<Texture2D>("square_lightblue");        
        Id = id;
        Width = 64;
        Height = 64;
        Player = player;        

        Position = position;

        if (Player is null)
        {
            CircleColor = Color.Red;
        }
        else
        {
            CircleColor = Player.Authorized ? Color.Green : Color.Red;
        }        

        _font = Game1.CurrentGame.MainFont;
    }

    public void Update(GameTime gameTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case BuildingBuiltState.Foundation:
                spriteBatch.FillRectangle(Position, new Size2(Width, Height), Color.LightGray);
                spriteBatch.DrawRectangle(Position, new Size2(Width, Height), Player.Color);
                break;
            case BuildingBuiltState.BeBuilt:
                break;
            case BuildingBuiltState.Built:
                spriteBatch.FillRectangle(Position, new Size2(Width, Height), Player.Color);
                spriteBatch.DrawRectangle(Position, new Size2(Width, Height), Color.LightGray, 3);
                break;
            default:
                break;
        }

        spriteBatch.DrawString(_font, Id.ToString(), CenterPosition, Color.White);

        if (IsSelected)
            spriteBatch.DrawCircle(CenterPosition, 50, 24, CircleColor, 1);
    }

    public bool TrySelect(RectangleF mouseClick)
    {
        IsSelected = false;
        if (Rectangle.Intersects(mouseClick))
        {
            IsSelected = true;
        }

        return IsSelected;
    }
}

public enum BuildingBuiltState
{
    Foundation = 0,
    BeBuilt = 1,
    Built = 2
}