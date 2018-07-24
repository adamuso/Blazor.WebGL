using Blazor.WebGL;
using Blazor.WebGL.Math;

namespace WebGame.Graphics
{
    public struct DefaultVertexFormat : IVertexFormat
    {
        public Vector3 Position { get; set; }
        public Vector2 UV { get; set; }
        public Color Color { get; set; }
        public float SamplerIndex { get; set; }
    }
}