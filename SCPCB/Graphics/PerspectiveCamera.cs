using System.Numerics;

namespace SCPCB.Graphics;

public interface ICamera {
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    Matrix4x4 GetViewMatrix(float interp);
}

// This camera interpolates the position, but keeps the rotation dynamic,
// which is what we need for our player camera.
public class PerspectiveCamera : ICamera {
    private Vector3 _prevPos;
    public Vector3 Position { get; set; }

    public Quaternion Rotation { get; set; }

    public Matrix4x4 GetViewMatrix(float interp) => CalculateMatrix(Vector3.Lerp(_prevPos, Position, interp), Rotation);

    public void UpdatePosition() {
        _prevPos = Position;
    }

    private static Matrix4x4 CalculateMatrix(Vector3 pos, Quaternion rot) {
        var forward = Vector3.Transform(Vector3.UnitZ, rot);
        var up = Vector3.Transform(Vector3.UnitY, rot);
        return Matrix4x4.CreateLookAt(pos, pos + forward, up);
    }
}
