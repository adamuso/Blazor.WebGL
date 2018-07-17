using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blazor.WebGL
{
    public class ActiveTexture2DCollection : IReadOnlyList<Texture2D>
    {
        private readonly WebGLContext context;
        private Texture2D[] textures;

        public int Count => textures.Length;

        internal ActiveTexture2DCollection(WebGLContext context, int size)
        {
            this.context = context;
            this.textures = new Texture2D[size];
        }

        public Texture2D this[int index] 
        {
            get { return textures[index]; }
            set 
            { 
                context.ActiveTexture(WebGLTextureIndex.TEXTURE0 + index);
                value.Bind();
                textures[index] = value; 
            }        
        }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            return textures.OfType<Texture2D>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() 
        {
            return GetEnumerator();
        }
    }
}