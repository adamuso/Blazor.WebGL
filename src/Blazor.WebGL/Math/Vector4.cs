namespace Blazor.WebGL.Math
{
    public struct Vector4
    {
        public static readonly Vector4 One = new Vector4(1, 1, 1, 1);
        public static readonly Vector4 Zero = new Vector4(0, 0, 0, 1);
     
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

        public Vector4 Transform(ref Matrix4 matrix) 
        {
            Vector4 @out = new Vector4();  
            float x = X, y = Y, z = Z, w = W;
            
            @out.X = matrix.m11 * x + matrix.m21 * y + matrix.m31 * z + matrix.m41 * w;
            @out.Y = matrix.m12 * x + matrix.m22 * y + matrix.m32 * z + matrix.m42 * w;
            @out.Z = matrix.m13 * x + matrix.m23 * y + matrix.m33 * z + matrix.m43 * w;
            @out.W = matrix.m14 * x + matrix.m24 * y + matrix.m34 * z + matrix.m44 * w;
            
            return @out;
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }
    }
}