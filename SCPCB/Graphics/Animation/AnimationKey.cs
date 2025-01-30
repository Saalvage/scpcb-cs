using System.Diagnostics;
using Assimp;
using System.Numerics;
using SCPCB.Utility;

namespace SCPCB.Graphics.Animation;

public interface IAnimationKey<T, TVal> where T : unmanaged, IAnimationKey<T, TVal> where TVal : unmanaged {
    public TVal Value { get; }
    public float Time { get; }

    static abstract TVal Interp(TVal a, TVal b, float interp);

    static TVal CalculateInterpolatedValue(IReadOnlyList<T> values, float time) {
        // This could likely be accelerated further, but it's probably good enough for now.
        var index = values.BinarySearch(time, (a, b) => a.Time.CompareTo(b));
        if (index < 0) {
            index = ~index;
        }

        if (index == 0) {
            return values[0].Value;
        } else if (index == values.Count) {
            return values[^1].Value;
        } else {
            var curr = values[index - 1];
            var next = values[index];
            Debug.Assert(curr.Time <= time);
            Debug.Assert(next.Time >= time);
            var interp = (time - curr.Time) / (next.Time - curr.Time);
            Debug.Assert(interp is >= 0 and <= 1);
            return T.Interp(curr.Value, next.Value, interp);
        }
    }
}

public record struct VectorAnimationKey(Vector3 Value, float Time) : IAnimationKey<VectorAnimationKey, Vector3> {
    public VectorAnimationKey(VectorKey key) : this(key.Value, (float)key.Time) { }
    public static Vector3 Interp(Vector3 a, Vector3 b, float interp)
        => Vector3.Lerp(a, b, interp);
    public static Vector3 CalculateInterpolatedValue(IReadOnlyList<VectorAnimationKey> values, float time)
        => IAnimationKey<VectorAnimationKey, Vector3>.CalculateInterpolatedValue(values, time);
}

public record struct QuaternionAnimationKey(Quaternion Value, float Time) : IAnimationKey<QuaternionAnimationKey, Quaternion> {
    public QuaternionAnimationKey(QuaternionKey key) : this(key.Value, (float)key.Time) { }
    public static Quaternion Interp(Quaternion a, Quaternion b, float interp)
        => Quaternion.Slerp(a, b, interp);
    public static Quaternion CalculateInterpolatedValue(IReadOnlyList<QuaternionAnimationKey> values, float time)
        => IAnimationKey<QuaternionAnimationKey, Quaternion>.CalculateInterpolatedValue(values, time);
}
