namespace Blazor.WebGL
{
    public struct VertexAttributeFormat
    {
        public int ComponentCount { get; set; }
        public WebGLType ComponentType { get; set; }
        public bool IsNormalized { get; set; }
        public int Stride { get; set; }
        public int Offset { get; set; }
    }
}