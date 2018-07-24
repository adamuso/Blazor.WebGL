namespace Blazor.WebGL.Math
{
    public struct Vector3
    {
        public static readonly Vector3 One = new Vector3(1, 1, 1);
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(Vector2 vec, float z)
        {
            X = vec.X;
            Y = vec.Y;
            Z = z;
        }

        
        public Vector3 Transform(ref Matrix4 matrix) 
        {
            Vector3 @out = new Vector3();  
            float x = X, y = Y, z = Z, w = 1;
            
            @out.X = matrix.m11 * x + matrix.m21 * y + matrix.m31 * z + matrix.m41 * w;
            @out.Y = matrix.m12 * x + matrix.m22 * y + matrix.m32 * z + matrix.m42 * w;
            @out.Z = matrix.m13 * x + matrix.m23 * y + matrix.m33 * z + matrix.m43 * w;
            
            return @out;
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        public static Vector3 operator +(Vector3 vec1, Vector3 vec2)
        {
            vec1.X += vec2.X;
            vec1.Y += vec2.Y;
            vec1.Z += vec2.Z;

            return vec1;
        }

        public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
        {
            vec1.X -= vec2.X;
            vec1.Y -= vec2.Y;
            vec1.Z -= vec2.Z;

            return vec1;
        }

        public static Vector3 operator *(Vector3 vec1, Vector3 vec2)
        {
            vec1.X *= vec2.X;
            vec1.Y *= vec2.Y;
            vec1.Z *= vec2.Z;

            return vec1;
        }

        public static Vector3 operator /(Vector3 vec1, Vector3 vec2)
        {
            vec1.X /= vec2.X;
            vec1.Y /= vec2.Y;
            vec1.Z /= vec2.Z;

            return vec1;
        }

        public static Vector3 operator *(Vector3 vec1, float scalar)
        {
            vec1.X *= scalar;
            vec1.Y *= scalar;
            vec1.Z *= scalar;

            return vec1;
        }

        public static Vector3 operator /(Vector3 vec1, float scalar)
        {
            vec1.X /= scalar;
            vec1.Y /= scalar;
            vec1.Z /= scalar;

            return vec1;
        }

        public static Vector3 operator -(Vector3 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;

            return vec;
        }
    }
}