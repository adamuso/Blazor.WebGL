namespace Blazor.WebGL
{
    public class WebGLShader
    {
        private WebGLContext context;
        public int Id { get; private set; }

        internal WebGLShader(WebGLContext context, int id)
        {
            this.context = context;
            Id = id;
        }
    }
}