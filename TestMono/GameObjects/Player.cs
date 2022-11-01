using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace TestMono.GameObjects;

internal class Player
{
    public bool Authorized { get; set; }
    public List<IUnit> Units { get; set; }

    public Player(bool authorized)
    {
        Authorized = authorized;

        Units = new List<IUnit>();

        var startUnit = new SimpleUnit(this);
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

            unit.EndPosition = new Vector2(endPosition.X - unit.Texture.Width / 2, endPosition.Y - unit.Texture.Height / 2);
        }
    }
}
