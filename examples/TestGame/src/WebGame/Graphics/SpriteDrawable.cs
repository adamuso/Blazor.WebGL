using System.Collections.Generic;
using Blazor.WebGL;
using Blazor.WebGL.Math;

namespace WebGame.Graphics
{
    public class SpriteDrawable : IDrawable<DefaultVertexFormat>
    {
        private static readonly Rectangle FullTexture = new Rectangle() { X = 0, Y = 0, Width = 1, Height = 1 };
        
        private DefaultVertexFormat[] cache;

        public SpriteDrawable()
        {
            cache = new DefaultVertexFormat[4];
        }

        public Matrix4 Transform;
        public Rectangle? SourceRectangle { get; set; }
        public Vector2 Size { get; set; }
        public Color Color { get; set; } 
        public Texture2D Texture { get; set; }

        public DefaultVertexFormat[] Draw()
        {
            cache[0].Position = Vector3.Zero.Transform(ref Transform);
            cache[1].Position = new Vector3(Size.X, 0, 0).Transform(ref Transform);
            cache[2].Position = new Vector3(Size.X, Size.Y, 0).Transform(ref Transform);
            cache[3].Position = new Vector3(0, Size.Y, 0).Transform(ref Transform);

            Rectangle sourceRectangle;

            if(SourceRectangle == null)
                sourceRectangle = FullTexture;
            else
                sourceRectangle = SourceRectangle.Value;

            cache[0].UV = new Vector2(sourceRectangle.X, sourceRectangle.Y);
            cache[1].UV = new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y);
            cache[2].UV = new Vector2(sourceRectangle.X + sourceRectangle.Width,
                sourceRectangle.Y + sourceRectangle.Height); 
            cache[3].UV = new Vector2(sourceRectangle.X, sourceRectangle.Y + sourceRectangle.Height);

            cache[0].Color = cache[1].Color = cache[2].Color = cache[3].Color = Color;

            return cache;
        }
    }
}