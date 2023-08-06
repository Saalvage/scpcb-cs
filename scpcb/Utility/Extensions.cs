using System.Numerics;
using Assimp;

namespace scpcb.Utility; 

public static class Extensions {
    public static IEnumerable<T> AsEnumerableElementOrEmpty<T>(this T? item) {
        if (item != null) {
            yield return item;
        }
    }

    public static IEnumerable<T> AsEnumerableElement<T>(this T item) {
        yield return item;
    }

    public static Vector2 ToCS(this Vector2D v) => new(v.X, v.Y);
    public static Vector3 ToCS(this Vector3D v) => new(v.X, v.Y, v.Z);
    public static Vector3 ToCS(this Color3D v) => new(v.R, v.G, v.B);
    public static Vector4 ToCS(this Color4D v) => new(v.R, v.G, v.B, v.A);
}
