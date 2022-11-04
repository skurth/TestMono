using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TestMono.GameObjects.Units;

namespace TestMono.GameObjects.Weapons;

public class Arrow
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 Position { get; set; }

    public Vector2 CenterPosition { get => new Vector2(Position.X + Width / 2, Position.Y + Height / 2); }

    public House House { get; set; }

    public RectangleF Rectangle { get => new RectangleF(Position.X, Position.Y, Width, Height); }

    public Vector2 EndPosition { get; set; }

    public float MovementSpeed { get; set; }

    public Arrow(House house, Vector2 startPosition, Vector2 endPosition)
    {
        Width = 12;
        Height = 12;
        House = house;
        MovementSpeed = 50.0f;
        EndPosition = Vector2.Zero;

        Position = startPosition;
        EndPosition = EndPosition;
    }
}
