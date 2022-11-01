using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TestMono.GameObjects.Map;

internal class Map
{
    private const int _MAP_TILES_X = 36;
    private const int _MAP_TILES_Y = 24;
    
    public List<MapTile> Tiles { get; set; }
    public Vector2 MaxMapSize { get; set; }

    public Map()
    {
        Tiles = new List<MapTile>();
        InitMap();
    }

    private void InitMap()
    {
        for (int idxX = 0; idxX < _MAP_TILES_X; idxX++)
        {
            for (int idxY = 0; idxY < _MAP_TILES_Y; idxY++)
            {
                Tiles.Add(new MapTile(idxX, idxY));                
            }
        }

        MaxMapSize = new Vector2(_MAP_TILES_X * MapTile._TILE_WIDTH, _MAP_TILES_Y * MapTile._TILE_HEIGHT);
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
}
