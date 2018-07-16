namespace Blazor.WebGL.Math
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        public float[] ToArray()
        {
            return new float[] { X, Y };    
        }
    }
}