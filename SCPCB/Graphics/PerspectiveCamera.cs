using System.Numerics;
using SCPCB.Utility;

namespace SCPCB.Graphics;

public interface ICamera : ITransformable {
    Matrix4x4 GetViewMatrix(float interp);
}

// This camera interpolates the position, but keeps the rotation dynamic,
// which is what we need for our player camera.
public class PerspectiveCamera : ICamera {
    private Vector3 _prevPos;

    public Transform WorldTransform { get; set; }

    public Transform GetInterpolatedWorldTransform(float interp) => WorldTransform with { Position = Vector3.Lerp(_prevPos, WorldTransform.Position, interp) };

    public Matrix4x4 GetViewMatrix(float interp) => CalculateMatrix(Vector3.Lerp(_prevPos, WorldTransform.Position, interp), WorldTransform.Rotation);

    public void UpdatePosition() {
        _prevPos = WorldTransform.Position;
    }

    private static Matrix4x4 CalculateMatrix(Vector3 pos, Quaternion rot) {
        var forward = Vector3.Transform(Vector3.UnitZ, rot);
        var up = Vector3.Transform(Vector3.UnitY, rot);
        return Matrix4x4.CreateLookAt(pos, pos + forward, up);
    }
}
