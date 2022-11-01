using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TestMono.GameObjects;

internal class SimpleUnit : IUnit
{    
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public Player Player { get; set; }
    public bool IsSelected { get; set; }

    private Vector2 CenterPosition { get => new Vector2(Position.X + Texture.Width / 2, Position.Y + Texture.Height / 2); }

    public RectangleF Rectangle { get => new RectangleF(Position.X, Position.Y, Texture.Width, Texture.Height); }

    public Vector2 EndPosition { get; set; }
    
    public float MovementSpeed { get; set; }

    public Color CircleColor { get; set; }


    public SimpleUnit(Player player)
    {        
        Texture = Game1.CurrentGame.Content.Load<Texture2D>("square_lightblue");        
        Player = player;
        MovementSpeed = 200.0f;
        EndPosition = Vector2.Zero;        

        if (Player.Authorized)
        {
            Position = new Vector2(100, 200);
            CircleColor = Color.Green;
        }
        else
        {
            Position = new Vector2(400, 500);
            CircleColor = Color.Red;
        }        
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
        spriteBatch.Draw(Texture, Position, Color.White);

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
