namespace Blazor.WebGL.Math
{
    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(float x, float y, float z, float w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }
    }
}