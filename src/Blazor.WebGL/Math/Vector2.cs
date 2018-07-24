namespace Blazor.WebGL.Math
{
    public struct Vector2
    {
        public static readonly Vector2 One = new Vector2(1, 1);
        public static readonly Vector2 Zero = new Vector2(0, 0);


        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        public Vector2 Transform(ref Matrix4 matrix) 
        {
            Vector2 @out = new Vector2();  
            float x = X;
            float y = Y;

            @out.X = matrix.m11 * x + matrix.m21 * y + matrix.m41;
            @out.Y = matrix.m12 * x + matrix.m22 * y + matrix.m42;
            
            return @out;
        }

        public float[] ToArray()
        {
            return new float[] { X, Y };    
        }

        public static Vector2 operator +(Vector2 vec1, Vector2 vec2)
        {
            vec1.X += vec2.X;
            vec1.Y += vec2.Y;

            return vec1;
        }

        public static Vector2 operator -(Vector2 vec1, Vector2 vec2)
        {
            vec1.X -= vec2.X;
            vec1.Y -= vec2.Y;

            return vec1;
        }

        public static Vector2 operator *(Vector2 vec1, Vector2 vec2)
        {
            vec1.X *= vec2.X;
            vec1.Y *= vec2.Y;

            return vec1;
        }

        public static Vector2 operator /(Vector2 vec1, Vector2 vec2)
        {
            vec1.X /= vec2.X;
            vec1.Y /= vec2.Y;

            return vec1;
        }

        public static Vector2 operator *(Vector2 vec1, float scalar)
        {
            vec1.X *= scalar;
            vec1.Y *= scalar;

            return vec1;
        }

        public static Vector2 operator /(Vector2 vec1, float scalar)
        {
            vec1.X /= scalar;
            vec1.Y /= scalar;

            return vec1;
        }

        public static Vector2 operator -(Vector2 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;

            return vec;
        }
    }
}