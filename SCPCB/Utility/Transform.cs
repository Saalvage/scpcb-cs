﻿using System.Diagnostics;
using System.Numerics;
using BepuPhysics;

namespace SCPCB.Utility;

// TODO: Consider turning this into a regular ol' struct.. The inability to modify properties is annoying.
public record struct Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale) {
    public Transform(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.One) { }
    public Transform(Vector3 position) : this(position, Quaternion.Identity) { }
    public Transform() : this(Vector3.Zero) { }

    public Matrix4x4 GetMatrix()
        => Matrix4x4.CreateScale(Scale)
         * Matrix4x4.CreateFromQuaternion(Rotation)
         * Matrix4x4.CreateTranslation(Position);

    public static Transform Lerp(Transform a, Transform b, float amount) {
        Debug.Assert(amount is >= 0 and <= 1);
        return new(
            Vector3.Lerp(a.Position, b.Position, amount),
            Quaternion.Slerp(a.Rotation, b.Rotation, amount),
            Vector3.Lerp(a.Scale, b.Scale, amount)
        );
    }

    /// <returns>
    /// Transform representing the application of two transforms in series.
    /// </returns>
    public static Transform operator+(Transform a, Transform b)
        => new(Vector3.Transform(b.Position * a.Scale, a.Rotation) + a.Position,
            a.Rotation * b.Rotation,
            a.Scale * b.Scale);

    /// <returns>
    /// The required transform to get from b to a.
    /// </returns>
    public static Transform operator-(Transform a, Transform b)
        => a + (-b);

    public static Transform operator-(Transform trans) {
        var inv = Quaternion.Inverse(trans.Rotation);
        return new(Vector3.Transform(-trans.Position, inv) / trans.Scale,
            inv,
            Vector3.One / trans.Scale);
    }
}

public static class TransformExtensions {
    public static Transform ToTransform(this RigidPose pose) => new(pose.Position, pose.Orientation);
}
