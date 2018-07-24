using System.Collections.Generic;
using Blazor.WebGL;
using Blazor.WebGL.Math;

namespace WebGame.Graphics
{
    public class Material
    {
        public WebGLShaderProgram Program { get; set; }
        public Texture2D[] Textures { get; set; }  
        public Dictionary<string, float> FloatUniforms { get; set; }
        public Dictionary<string, Vector3> Vector3Uniforms { get; set; }
        public Dictionary<string, Matrix4> Matrix4Uniforms { get; set; }
        public Dictionary<string, int> Int32Uniforms { get; set; }
    }
}