using Microsoft.Xna.Framework;

namespace TestMono.Scenes;

public interface IScene
{
    public void Update(GameTime gameTime);
    public void Draw(GameTime gameTime);
}
