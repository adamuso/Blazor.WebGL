using Blazor.WebGL.Math;

namespace Blazor.WebGL
{
    public class WebGLUniformLocation : ContextObject
    {
        internal WebGLShaderProgram Program { get; } 
        internal string Name { get; }

        internal WebGLUniformLocation(WebGLShaderProgram program, string name)
            : base(ContextType.UniformLocation, new object[] { program.Id, name })
        {
            this.Program = program;
            this.Name = name;
        }

        public void Set(ref Matrix4 matrix, bool transpose = false)
        {
            Program.Context.UniformMatrix4fv(this, transpose, ref matrix);
        }

        public void Set(float value)
        {
            Program.Context.SetProgramUniform(this, 1, "f", value);
        }

        public void Set(Vector2 vector)
        {
            Program.Context.SetProgramUniform(this, 2, "f", vector.ToArray());            
        }

        public void Set(Vector3 vector)
        {
            Program.Context.SetProgramUniform(this, 3, "f", vector.ToArray());                        
        }

        public void Set(Vector4 vector)
        {
            Program.Context.SetProgramUniform(this, 4, "f", vector.ToArray());                        
        }

        public void Set(int value)
        {
            Program.Context.SetProgramUniform(this, 1, "i", value);
        }

        public void Set(int v1, int v2)
        {
            Program.Context.SetProgramUniform(this, 2, "i", new int[] { v1, v2 });        
        }

        public void Set(int v1, int v2, int v3)
        {
            Program.Context.SetProgramUniform(this, 3, "i", new int[] { v1, v2, v3 });                    
        }

        public void Set(int v1, int v2, int v3, int v4)
        {
            Program.Context.SetProgramUniform(this, 4, "i", new int[] { v1, v2, v3, v4 });                                
        }
    }
}