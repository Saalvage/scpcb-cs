using Assimp;
using System.Numerics;

namespace SCPCB.Graphics.Animation;

public interface IAnimationKey<T, TVal> where T : unmanaged, IAnimationKey<T, TVal> where TVal : unmanaged {
    public TVal Value { get; }
    public float Time { get; }

    static abstract TVal Interp(TVal a, TVal b, float interp);

    static TVal CalculateInterpolatedValue(IReadOnlyList<T> values, float time) {
        // TODO: This has actual performance implications and can be optimized further.
        var scaleKeys = values.Zip(values.Skip(1))
            .Cast<(T, T)?>()
            .FirstOrDefault(x => time < x!.Value.Item2.Time);
        if (scaleKeys.HasValue) {
            var (curr, next) = scaleKeys.Value;
            var interp = (time - curr.Time) / (next.Time - curr.Time);
            return T.Interp(curr.Value, next.Value, interp);
        } else {
            return values[^1].Value;
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
