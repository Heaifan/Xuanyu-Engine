using System.Numerics;

namespace XuanYu.Editor.UI.Right;

public static class TransformEuler
{
    private const float ToRadians = MathF.PI / 180f;
    private const float ToDegreesFactor = 180f / MathF.PI;

    public static Quaternion FromDegrees(Vector3 degrees)
    {
        Vector3 radians = degrees * ToRadians;
        Matrix4x4 matrix =
            Matrix4x4.CreateRotationX(radians.X) *
            Matrix4x4.CreateRotationY(radians.Y) *
            Matrix4x4.CreateRotationZ(radians.Z);
        return Quaternion.CreateFromRotationMatrix(matrix);
    }

    public static Vector3 ToDegrees(Quaternion rotation)
    {
        Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(rotation);
        float y = MathF.Asin(Math.Clamp(-matrix.M13, -1f, 1f));
        float x;
        float z;
        if (MathF.Abs(matrix.M13) > 0.9999f)
        {
            x = MathF.Atan2(-matrix.M32, matrix.M22);
            z = 0f;
        }
        else
        {
            x = MathF.Atan2(matrix.M23, matrix.M33);
            z = MathF.Atan2(matrix.M12, matrix.M11);
        }
        return new Vector3(x, y, z) * ToDegreesFactor;
    }
}