using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;

namespace scpcb.Graphics;

public interface ICamera : IConstantProvider<IViewMatrixConstantMember, Matrix4x4>, IConstantProvider<IViewPositionConstantMember, Vector3> {
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    Matrix4x4 ViewMatrix { get; }

    Matrix4x4 IConstantProvider<IViewMatrixConstantMember, Matrix4x4>.GetValue(float interp) => ViewMatrix;
    Vector3 IConstantProvider<IViewPositionConstantMember, Vector3>.GetValue(float interp) => Position;
};

public class PerspectiveCamera : ICamera {
    private Vector3 _pos;
    public Vector3 Position {
        get => _pos;
        set {
            _outdated = true;
            _pos = value;
        }
    }

    private Quaternion _rot = Quaternion.Identity;
    public Quaternion Rotation {
        get => _rot;
        set {
            _outdated = true;
            _rot = value;
        }
    }

    private bool _outdated = true;
    private Matrix4x4 _viewMat;
    public Matrix4x4 ViewMatrix => _outdated ? Update() : _viewMat;

    private ref Matrix4x4 Update() {
        var forward = Vector3.Transform(Vector3.UnitZ, _rot);
        var up = Vector3.Transform(Vector3.UnitY, _rot);
        _viewMat = Matrix4x4.CreateLookAt(Position, Position + forward, up);
        _outdated = false;
        return ref _viewMat;
    }

    // TODO: Instead of this, consider using multiple constant providers that get created in-place.
    public void ApplyTo(IEnumerable<IConstantHolder?> holders, float interp) {
        ((IConstantProvider<IViewMatrixConstantMember, Matrix4x4>)this).ApplyToInternal(holders, interp);
        ((IConstantProvider<IViewPositionConstantMember, Vector3>)this).ApplyToInternal(holders, interp);
    }
}
