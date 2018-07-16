namespace Blazor.WebGL
{
    public class WebGLAttributeLocation
    {
        private WebGLShaderProgram program;
        private long id;

        internal WebGLAttributeLocation(WebGLShaderProgram program, long id)
        {
            this.program = program;
            this.id = id;
        }

        public void VertexAttributePointer(int size, WebGLType type, bool normalized, int stride, int offset)
        {
            program.Context.VertexAttributePointer(id, size, type, normalized, stride, offset);
        }

        public void EnableVertexAttributeArray()
        {
            program.Context.EnableVertexAttributeArray(id);
        }
    }
}