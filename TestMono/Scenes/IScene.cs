using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMono.Scenes;

internal interface IScene
{
    public void Update(GameTime gameTime);
    public void Draw(GameTime gameTime);
}
