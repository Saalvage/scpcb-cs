using System.Numerics;

namespace scpcb;

public record struct Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale) {
    public Transform() : this(Vector3.Zero, Quaternion.Identity, Vector3.One) { }

    public Matrix4x4 GetMatrix()
        => Matrix4x4.CreateTranslation(Position)
         * Matrix4x4.CreateFromQuaternion(Rotation)
         * Matrix4x4.CreateScale(Scale);
}
