using System.Linq;

namespace Blazor.WebGL
{
    public class Texture2D
    {
        private WebGLContext context;

        internal int Id { get; private set; }

        public int Width { get; }
        public int Height { get; }
        public PixelFormat Format { get; }

        internal Texture2D(WebGLContext context, int id, int width, int height)
        {
            this.context = context;
            Id = id;
            Width = width;
            Height = height;
            Format = PixelFormat.RGBA;
        }

        public Texture2D(WebGLContext context, int width, int height)
        {
            this.context = context;
            Id = context.CreateTexture(width, height, PixelFormat.RGBA);
            Width = width;
            Height = height;
            Format = PixelFormat.RGBA;
        }

        public Texture2D(WebGLContext context, int width, int height, PixelFormat format)
        {
            this.context = context;
            Id = context.CreateTexture(width, height, format);
            Width = width;
            Height = height;
            Format = format;
        }

        public void SetData(Color[] data)
        {
            context.SetTextureData(this, Width, Height, Format, PixelFormat.RGBA, PixelType.UNSIGNED_BYTE, data.Select(d => (int)d.ToUInt32()).ToArray());
        }

        public void Bind()
        {
            context.BindTexture(this);
        }
    }
}