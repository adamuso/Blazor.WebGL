using Blazor.WebGL;
using WebGame.Graphics;

namespace WebGame
{
    public class SpriteBatch : RenderBatch<DefaultVertexFormat, DefaultVertexFormatDescriptor>
    {
        public SpriteBatch(WebGLContext context, int elementCount)
            : base(context, elementCount)
        {

        }
    }
}