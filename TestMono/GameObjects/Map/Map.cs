using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TestMono.GameObjects.Map;

public class Map
{
    public const int _MAP_MIN_TILES_X = 5;
    public const int _MAP_MAX_TILES_X = 12;
    public const int _MAP_MIN_TILES_Y = 5;
    public const int _MAP_MAX_TILES_Y = 10;

    public List<MapTile> Tiles { get; set; }
    public Vector2 MaxMapSize { get; set; }

    public int MaxTileX { get; set; }
    public int MaxTileY { get; set; }

    public Map()
    {
        Tiles = new List<MapTile>();
    }

    public void InitMap(int tilesX, int tilesY)
    {
        for (int idxX = 0; idxX < tilesX; idxX++)
        {
            for (int idxY = 0; idxY < tilesY; idxY++)
            {
                Tiles.Add(new MapTile(idxX, idxY));                
            }
        }

        MaxTileX = tilesX;
        MaxTileY = tilesY;
        MaxMapSize = new Vector2(tilesX * MapTile._TILE_WIDTH, tilesY * MapTile._TILE_HEIGHT);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var tile in Tiles)
        {
            tile.Draw(spriteBatch);
        }
    }

    public bool IsInside(Vector2 position)
    {
        return position.X > 0 && position.Y > 0 && position.X < MaxMapSize.X && position.Y < MaxMapSize.Y;
    }

    public static Vector2 GetPositionFromTiles(int tileX, int tileY)
    {
        return new Vector2(tileX * MapTile._TILE_WIDTH, tileY * MapTile._TILE_HEIGHT);
    }
}
