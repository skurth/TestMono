using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TestMono.GameObjects.Map;

public class MapTile
{
    public const int _TILE_WIDTH = 64;
    public const int _TILE_HEIGHT = 64;

    public Vector2 Position { get; set; }

    public Texture2D Texture { get; set; }

    private int _x = 0;
    private int _y = 0;

    public MapTile(int X, int Y)
    {
        _x = X; _y = Y;
        Position = new Vector2(X * _TILE_WIDTH, Y * _TILE_HEIGHT);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.FillRectangle(Position, new Size2(_TILE_WIDTH, _TILE_HEIGHT), Color.ForestGreen);
        spriteBatch.DrawRectangle(Position, new Size2(_TILE_WIDTH, _TILE_HEIGHT), Color.LightGray, 0.5f);
    }
}
