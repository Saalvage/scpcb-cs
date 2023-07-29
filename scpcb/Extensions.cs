using System.Linq;
using System.Numerics;
using Assimp;
using scpcb.Collision;
using Vortice.DXGI;
using Plane = System.Numerics.Plane;

namespace scpcb; 

public static class Extensions {
    public static IEnumerable<T> FromSingleOrEmpty<T>(T? item) {
        if (item != null) {
            yield return item;
        }
    }

    public static IEnumerable<T> CumSum<T, TSelector>(this IEnumerable<TSelector> enumerable, Func<TSelector, T> selector) where T : INumber<T> {
        var sum = default(T);
        foreach (var item in enumerable.Select(selector)) {
            sum += item;
            yield return sum;
        }
    }

    public static IEnumerable<T> CumSum<T>(this IEnumerable<T> enumerable) where T : INumber<T> {
        var sum = default(T);
        foreach (var item in enumerable) {
            sum += item;
            yield return sum;
        }
    }

    public static Vector2 ToCS(this Vector2D v) => new(v.X, v.Y);
    public static Vector3 ToCS(this Vector3D v) => new(v.X, v.Y, v.Z);
    public static Vector3 ToCS(this Color3D v) => new(v.R, v.G, v.B);
    public static Vector4 ToCS(this Color4D v) => new(v.R, v.G, v.B, v.A);


    public static Vector2 XY(this Vector3 vec) => new(vec.X, vec.Y);
    public static Vector2 XY(this Vector4 vec) => new(vec.X, vec.Y);
    public static Vector3 XYZ(this Vector4 vec) => new(vec.X, vec.Y, vec.Z);

    public static Vector3 Denormalize(this Vector4 vec) => vec.XYZ() / vec.W;

    public static bool Intersects(this Plane plane, Vector3 begin, Vector3 end, out Vector3 intersectionPoint, out float coveredAmount, bool ignoreDirection = false, bool ignoreSegment = false) {
        // http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm#Line%20Intersections
        // http://paulbourke.net/geometry/planeline/

        intersectionPoint = Vector3.Zero;
        coveredAmount = 0;

        Vector3 dir = end - begin;
        float denominator = -Vector3.Dot(plane.Normal, dir);
        float numerator = Vector3.Dot(plane.Normal, (begin + plane.Normal * -plane.D));
        if (Helpers.EqualFloats(denominator, 0) || (!ignoreDirection && denominator < 0)) { return false; }
        float u = numerator / denominator;
        if (!ignoreSegment && (u < 0 || u > 1)) { return false; }
        intersectionPoint = begin + dir * u;
        coveredAmount = u;
        return true;
    }

    public static float EvalAtPoint(this Plane plane, Vector3 co)
        => Plane.DotNormal(plane, co) + plane.D;

    public static CollideRRR.PointRelation OnPlane(this Plane plane, Vector3 co) {
        float res = plane.EvalAtPoint(co);
        //if (Helpers.EqualFloats(res, 0)) { return CollideRRR.PointRelation.ON; }
        if (res < 0) { return CollideRRR.PointRelation.BELOW; }
        return CollideRRR.PointRelation.ABOVE;
    }

    public static Vector3 SafeNormalize(this Vector3 vec) {
        float lenSqr = vec.LengthSquared();
        if (Helpers.EqualFloats(0, lenSqr)) { return new(); }
        if (Helpers.EqualFloats(1, lenSqr)) { return vec; }
        return vec / MathF.Sqrt(lenSqr);
    }
}
