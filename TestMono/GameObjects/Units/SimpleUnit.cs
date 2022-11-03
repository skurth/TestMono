using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TestMono.GameObjects.Units;

internal class SimpleUnit : IUnit
{
    //public Texture2D Texture { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 Position { get; set; }
    public Player Player { get; set; }
    public bool IsSelected { get; set; }

    private Vector2 CenterPosition { get => new Vector2(Position.X + Width / 2, Position.Y + Height / 2); }

    public RectangleF Rectangle { get => new RectangleF(Position.X, Position.Y, Width, Height); }

    public Vector2 EndPosition { get; set; }

    public float MovementSpeed { get; set; }

    public Color CircleColor { get; set; }

    private readonly SpriteFont _font;

    public SimpleUnit(int id, Player player, Vector2 startPosition)
    {
        //Texture = Game1.CurrentGame.Content.Load<Texture2D>("square_lightblue");        
        Id = id;
        Width = 48;
        Height = 48;
        Player = player;
        MovementSpeed = 200.0f;
        EndPosition = Vector2.Zero;

        Position = startPosition;

        CircleColor = Player.Authorized ? Color.Green : Color.Red;

        _font = Game1.CurrentGame.MainFont;
    }

    public void Update(GameTime gameTime)
    {
        if (EndPosition != Vector2.Zero)
        {
            var direction = Position - EndPosition;

            if (direction.Length() < MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                Position = EndPosition;
                EndPosition = Vector2.Zero;
                return;
            }

            direction.Normalize();

            Position -= direction * MovementSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        //spriteBatch.Draw(Texture, Position, Color.White);
        spriteBatch.FillRectangle(Position, new Size2(Width, Height), Player.Color);
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
