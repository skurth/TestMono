using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace TestMono.GameObjects;

public class Player
{
    public string Id { get; set; }
    public bool Authorized { get; set; }
    public List<IUnit> Units { get; set; }
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

    public void MoveSelectedUnit(Vector2 endPosition)
    {
        foreach (var unit in Units)
        {
            if (!unit.IsSelected) { continue; }

            unit.EndPosition = new Vector2(endPosition.X - unit.Width / 2, endPosition.Y - unit.Height / 2);
        }
    }
}
