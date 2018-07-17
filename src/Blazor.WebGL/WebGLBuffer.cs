namespace Blazor.WebGL
{
    public class WebGLBuffer
    {
        internal const int ContextType = 2;

        private WebGLContext context;

        public int Id { get; set; }
        public BufferType Type { get; set; }

        public WebGLBuffer(WebGLContext context, int id, BufferType type)
        {
            this.context = context;
            this.Id = id;
            this.Type = type;
        }

        public void Bind()
        {
            context.BindBuffer(this);
        }

        public void SetData(float[] data, BufferUsage usage)
        {
            context.BufferData(this, data, usage);
        }

        public void SetData(ushort[] data, BufferUsage usage)
        {
            context.BufferData(this, data, usage);
        }
    }
}