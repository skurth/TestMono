using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using TestMono.GameObjects.Units;

namespace TestMono.GameObjects;

public class Player
{
    public string Id { get; set; }
    public bool Authorized { get; set; }
    public List<IUnit> Units { get; set; }
    public List<IBuilding> Buildings { get; set; }
    public Color Color { get; set; }

    public Player(
        string id,
        bool authorized,
        int startUnitId,
        Vector2 startUnitPosition,
        Color color)
    {
        Id = id;
        Authorized = authorized;
        Color = color;

        Units = new List<IUnit>();
        Buildings = new List<IBuilding>();

        var startUnit = new SimpleUnit(startUnitId, this, startUnitPosition);
        Units.Add(startUnit);        
    }

    public void Update(GameTime gameTime)
    {
        foreach (var unit in Units)
        {
            unit.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var building in Buildings)
        {
            building.Draw(spriteBatch);
        }

        foreach (var unit in Units)
        {
            unit.Draw(spriteBatch);
        }        
    }

    public void UnselectAllUnits()
    {
        foreach (var unit in Units)
        {
            unit.IsSelected = false;
        }
    }

    public bool TrySelectUnit(RectangleF mouseClick)
    {
        foreach (var unit in Units)
        {
            if (unit.TrySelect(mouseClick))
            {
                return true;
            }
        }

        return false;
    }

    public void UnselectAllBuildings()
    {
        foreach (var building in Buildings)
        {
            building.IsSelected = false;
        }
    }

    public bool TrySelectBuilding(RectangleF mouseClick)
    {
        foreach (var building in Buildings)
        {
            if (building.TrySelect(mouseClick))
            {
                return true;
            }
        }

        return false;
    }
}
