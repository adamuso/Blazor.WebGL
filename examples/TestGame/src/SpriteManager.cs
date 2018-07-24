using System.Collections.Generic;

namespace WebGame
{
    public class SpriteManager
    {
        private List<Sprite> sprites;

        public SpriteManager()
        {
            sprites = new List<Sprite>();
        }

        public void Draw(SpriteBatch batch)
        {
            foreach(var sprite in sprites)
                batch.Draw(sprite.Drawable);
        }

        public void Update(float delta)
        {
            foreach(var sprite in sprites)
                sprite.Update(delta);
        }
    }
}