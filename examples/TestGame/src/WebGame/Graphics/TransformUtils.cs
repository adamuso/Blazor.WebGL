using Blazor.WebGL.Math;

namespace WebGame.Graphics
{
    public static class TransformUtils
    {
        public static Matrix4 MakeTransform(Vector3 origin, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Matrix4 matrix = Matrix4.Identity;
            matrix.Translate(-origin);
            matrix.Scale(scale);
            matrix.Rotate(rotation.X, new Vector3(1, 0, 0));
            matrix.Rotate(rotation.Y, new Vector3(0, 1, 0));
            matrix.Rotate(rotation.Z, new Vector3(0, 0, 1));
            matrix.Translate(position);

            return matrix;
        }
    }
}