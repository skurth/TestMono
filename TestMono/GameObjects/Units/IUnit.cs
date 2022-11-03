using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TestMono.GameObjects.Units;

public interface IUnit
{
    //public Texture2D Texture { get; set; }
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Vector2 Position { get; set; }
    public Player Player { get; set; }
    public bool IsSelected { get; set; }
    public RectangleF Rectangle { get; }

    public Vector2 EndPosition { get; set; }

    public float MovementSpeed { get; set; }
    public Color CircleColor { get; set; }

    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch);

    public bool TrySelect(RectangleF mouseClick);
}
