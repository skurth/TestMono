using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TestMono.GameObjects.Units;
using TestMono.Scenes;

namespace TestMono.GameObjects.Buildings;

public class BuildingsUtils
{
    public GameScene GameScene { get; }

    public int CurrentBuildingId { get; set; } = 0;

    public BuildingsUtils(GameScene gameScene)
    {
        GameScene = gameScene;
    }

    public IBuilding GetBuildingById(int id)
    {
        foreach (var player in GameScene.Players)
        {
            foreach (var building in player.Buildings)
            {
                if (building.Id == id)
                {
                    return building;
                }
            }
        }

        return null;
    }

    public int GetNextBuildingId()
    {
        return CurrentBuildingId++;
    }

    public bool CanBuildBuilding(float positionX, float positionY, int width, int height)
    {
        var targetBuilding = new RectangleF(positionX, positionY, width, height);

        foreach (var player in GameScene.Players)
        {
            foreach (var building in player.Buildings)
            {
                if (building.Rectangle.Intersects(targetBuilding))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
