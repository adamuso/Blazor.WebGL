using System.Collections.Generic;
using Blazor.WebGL;

namespace WebGame.Graphics
{
    public interface IDrawable<T> 
        where T : struct, IVertexFormat
    {
        Texture2D Texture { get; }
        T[] Draw();
    }
}