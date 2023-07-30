using System.Numerics;

namespace scpcb;

public record struct Transform(Vector3 Position, Quaternion Rotation, Vector3 Scale) {
    public Transform(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.One) { }
    public Transform() : this(Vector3.Zero, Quaternion.Identity, Vector3.One) { }

    public Matrix4x4 GetMatrix()
        => Matrix4x4.CreateScale(Scale) 
         * Matrix4x4.CreateFromQuaternion(Rotation) 
         * Matrix4x4.CreateTranslation(Position);

    public Transform Lerp(Transform other, float amount)
        => new(
            Vector3.Lerp(Position, other.Position, amount),
            Quaternion.Slerp(Rotation, other.Rotation, amount),
            Vector3.Lerp(Scale, other.Scale, amount)
        );
}
