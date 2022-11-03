using TestMono.Scenes;

namespace TestMono.GameObjects.Units;

public class UnitsUtils
{
    public GameScene GameScene { get; }

    public UnitsUtils(GameScene gameScene)
    {
        GameScene = gameScene;
    }

    public IUnit GetUnitById(int id)
    {
        foreach (var player in GameScene.Players)
        {
            foreach (var unit in player.Units)
            {
                if (unit.Id == id)
                {
                    return unit;
                }
            }
        }

        return null;
    }
}
