using Blazor.WebGL;
using Blazor.WebGL.Math;
using WebGame.Graphics;

namespace WebGame
{
    public class Sprite 
    {
        public TestGame Game { get; private set; }
        public Vector2 Position { get; set; }
        public Rectangle BoundingBox { get; set; }
        public SpriteDrawable Drawable { get; set; }

        public Sprite(TestGame game)
        {
            Game = game;
        }

        public virtual void Update(float delta)
        {

        }
    }
}