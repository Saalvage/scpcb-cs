using System.Diagnostics;
using System.Numerics;

namespace scpcb.Collision;

public struct AABB {
    private readonly bool _isValid = false;

    public Vector3 Min { private set; get; }
    public Vector3 Max { private set; get; }

    public AABB() {
        throw new InvalidOperationException("Must not be empty!");
    }

    public AABB(Vector3 pointInBox) {
        _isValid = true;

        Min = Max = pointInBox;
    }

    public AABB(IEnumerable<Vector3> points) {
        _isValid = true;

        using var enumerator = points.GetEnumerator();
        if (!enumerator.MoveNext()) {
            throw new ArgumentException("Must not be empty!", nameof(points));
        }

        Min = Max = enumerator.Current;

        while (enumerator.MoveNext()) {
            AddPoint(enumerator.Current);
        }
    }

    public AABB(IEnumerable<AABB> aabbs) {
        _isValid = true;

        using var enumerator = aabbs.GetEnumerator();
        if (!enumerator.MoveNext()) {
            throw new ArgumentException("Must not be empty!", nameof(aabbs));
        }

        Min = enumerator.Current.Min;
        Max = enumerator.Current.Max;

        while (enumerator.MoveNext()) {
            Min = Vector3.Min(Min, enumerator.Current.Min);
            Max = Vector3.Max(Max, enumerator.Current.Max);
        }
    }

    public void AddPoint(Vector3 point) {
        Debug.Assert(_isValid);

        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
    }

    public bool Contains(Vector3 point) {
        Debug.Assert(_isValid);

        return point.X > Min.X && point.X < Max.X
            && point.Y > Min.Y && point.Y < Max.Y
            && point.Z > Min.Z && point.Z < Max.Z;
    }

    public bool Intersects(AABB other) {
        Debug.Assert(_isValid);

        return Min.X <= other.Max.X && Max.X >= other.Min.X
            && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y
            && Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }
}
