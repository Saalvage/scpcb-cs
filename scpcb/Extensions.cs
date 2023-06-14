using System.Numerics;
using Assimp;

namespace scpcb; 

public static class Extensions {
    public static IEnumerable<T> FromSingleOrEmpty<T>(T? item) {
        if (item != null) {
            yield return item;
        }
    }

    public static Vector2 ToCS(this Vector2D v) => new(v.X, v.Y);
    public static Vector3 ToCS(this Vector3D v) => new(v.X, v.Y, v.Z);
    public static Vector3 ToCS(this Color3D v) => new(v.R, v.G, v.B);
    public static Vector4 ToCS(this Color4D v) => new(v.R, v.G, v.B, v.A);


    public static Vector2 XY(this Vector3 vec) => new(vec.X, vec.Y);
    public static Vector2 XY(this Vector4 vec) => new(vec.X, vec.Y);
    public static Vector3 XYZ(this Vector4 vec) => new(vec.X, vec.Y, vec.Z);
}
