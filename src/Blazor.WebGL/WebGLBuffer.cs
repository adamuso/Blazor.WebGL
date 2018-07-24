namespace Blazor.WebGL
{
    public class WebGLBuffer
    {
        internal const int ContextType = 2;

        private WebGLContext context;

        public int Id { get; private set; }
        public BufferType Type { get; private set; }
        public BufferUsage Usage { get; private set; }

        public WebGLBuffer(WebGLContext context, int id, BufferType type, BufferUsage usage)
        {
            this.context = context;
            this.Id = id;
            this.Type = type;
            this.Usage = usage;
        }

        public void Bind()
        {
            context.BindBuffer(this);
        }

        public void SetDataSize(int byteSize)
        {
            context.BufferData(this, byteSize);
        }

        public void SetData<T>(T[] data, int itemByteSize, int length)
            where T : struct
        {
            context.BufferData(this, data, itemByteSize, length);
        }

        public void SetData<T>(T[] data, int length)
            where T : struct
        {
            context.BufferData(this, data, System.Runtime.InteropServices.Marshal.SizeOf<T>(), length);
        }

        public void SetData(float[] data)
        {
            context.BufferData(this, data);
        }

        public void SetData(ushort[] data)
        {
            context.BufferData(this, data);
        }
    }
}