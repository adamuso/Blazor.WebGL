namespace Blazor.WebGL
{
    public class Texture2D
    {
        private WebGLContext context;

        internal int Id { get; private set; }

        internal Texture2D(WebGLContext context, int id)
        {
            this.context = context;
            this.Id = id;
        }

        public void Bind()
        {
            context.BindTexture(this);
        }
    }
}