namespace Blazor.WebGL.Math
{
    public struct Matrix4 
    {
        private const float Epsilon = 0.00001f;
        public static Matrix4 Identity { get; private set; }

        static Matrix4()
        {
            var identity = new Matrix4();
            identity.m11 = 1;
            identity.m12 = 0;
            identity.m13 = 0;
            identity.m14 = 0;
            identity.m21 = 0;
            identity.m22 = 1;
            identity.m23 = 0;
            identity.m24 = 0;
            identity.m31 = 0;
            identity.m32 = 0;
            identity.m33 = 1;
            identity.m34 = 0;
            identity.m41 = 0;
            identity.m42 = 0;
            identity.m43 = 0;
            identity.m44 = 1;

            Identity = identity;
        }


        public static Matrix4 Perspective(float fovy, float aspect, float near, float far) 
        {
            Matrix4 @out = new Matrix4();

            float f = 1.0f / (float)System.Math.Tan(fovy / 2.0f);
            float nf;
            @out.m11 = f / aspect;
            @out.m12 = 0;
            @out.m13 = 0;
            @out.m14 = 0;
            @out.m21 = 0;
            @out.m22 = f;
            @out.m23 = 0;
            @out.m24 = 0;
            @out.m31 = 0;
            @out.m32 = 0;
            // @out.m33 = out[10]
            @out.m34 = -1;
            @out.m41 = 0;
            @out.m42 = 0;
            // @out.m43 = out[14]
            @out.m44 = 0;

            if (!float.IsInfinity(far)) 
            {
                nf = 1 / (near - far);
                @out.m33 = (far + near) * nf;
                @out.m43 = (2 * far * near) * nf;
            }
            else 
            {
                @out.m33 = -1;
                @out.m43 = -2 * near;
            }
            
            return @out;
        }

        public static Matrix4 Orthogonal(float left, float top, float right, float bottom,
            float near, float far) 
        {
            float lr = 1 / (left - right);
            float bt = 1 / (bottom - top);
            float nf = 1 / (near - far);

            Matrix4 @out;

            @out.m11= -2 * lr;
            @out.m12 = 0;
            @out.m13= 0;
            @out.m14= 0;
            @out.m21= 0;
            @out.m22 = -2 * bt;
            @out.m23 = 0;
            @out.m24= 0;
            @out.m31= 0;
            @out.m32= 0;
            @out.m33 = 2 * nf;
            @out.m34= 0;
            @out.m41= (left + right) * lr;
            @out.m42= (top + bottom) * bt;
            @out.m43= (far + near) * nf;
            @out.m44= 1;

            return @out;
        }

        public float m11; // out[0]
        public float m12; // out[1]
        public float m13; // out[2]
        public float m14; // out[3]
        public float m21; // out[4]
        public float m22; // out[5]
        public float m23; // out[6]
        public float m24; // out[7]
        public float m31; // out[8]
        public float m32; // out[9]
        public float m33; // out[10]
        public float m34; // out[11]
        public float m41; // out[12]
        public float m42; // out[13]
        public float m43; // out[14]
        public float m44; // out[15]

        public void Translate(Vector3 vector) 
        {
            float x = vector.X; 
            float y = vector.Y;
            float z = vector.Z;

            this.m41 = this.m11 * x + this.m21 * y + this.m31 * z + this.m41;
            this.m42 = this.m12 * x + this.m22 * y + this.m32 * z + this.m42;
            this.m43 = this.m13 * x + this.m23 * y + this.m33 * z + this.m43;
            this.m44 = this.m14 * x + this.m24 * y + this.m34 * z  +this.m44;
        }   
        
        public void Rotate(float rad, Vector3 axis) 
        {
            float x = axis.X, y = axis.Y, z = axis.Z;
            float len = (float)System.Math.Sqrt(x * x + y * y + z * z);
            float s, c, t;
            float a00, a01, a02, a03;
            float a10, a11, a12, a13;
            float a20, a21, a22, a23;
            float b00, b01, b02;
            float b10, b11, b12;
            float b20, b21, b22;

            if (len < Epsilon) 
                return;

            len = 1 / len;
            x *= len;
            y *= len;
            z *= len;

            s = (float)System.Math.Sin(rad);
            c = (float)System.Math.Cos(rad);
            t = 1 - c;

            a00 = m11; a01 = m12; a02 = m13; a03 = m14;
            a10 = m21; a11 = m22; a12 = m23; a13 = m24;
            a20 = m31; a21 = m32; a22 = m33; a23 = m34;

            // Construct the elements of the rotation matrix
            b00 = x * x * t + c; b01 = y * x * t + z * s; b02 = z * x * t - y * s;
            b10 = x * y * t - z * s; b11 = y * y * t + c; b12 = z * y * t + x * s;
            b20 = x * z * t + y * s; b21 = y * z * t - x * s; b22 = z * z * t + c;

            // Perform rotation-specific matrix multiplication
            m11 = a00 * b00 + a10 * b01 + a20 * b02;
            m12 = a01 * b00 + a11 * b01 + a21 * b02;
            m13 = a02 * b00 + a12 * b01 + a22 * b02;
            m14 = a03 * b00 + a13 * b01 + a23 * b02;
            m21 = a00 * b10 + a10 * b11 + a20 * b12;
            m22 = a01 * b10 + a11 * b11 + a21 * b12;
            m23 = a02 * b10 + a12 * b11 + a22 * b12;
            m24 = a03 * b10 + a13 * b11 + a23 * b12;
            m31 = a00 * b20 + a10 * b21 + a20 * b22;
            m32 = a01 * b20 + a11 * b21 + a21 * b22;
            m33 = a02 * b20 + a12 * b21 + a22 * b22;
            m34 = a03 * b20 + a13 * b21 + a23 * b22;
        }

        public void Scale(Vector3 scale) 
        {
            float x = scale.X, y = scale.Y, z = scale.Z;

            m11 = m11 * x;
            m12 = m12 * x;
            m13 = m13 * x;
            m14 = m14 * x;
            m21 = m21 * y;
            m22 = m22 * y;
            m23 = m23 * y;
            m24 = m24 * y;
            m31 = m31 * z;
            m32 = m32 * z;
            m33 = m33 * z;
            m34 = m34 * z;
        }

        public float[] ToArray()
        {
            return new float[] { 
                m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44
            };
        }
    }
}