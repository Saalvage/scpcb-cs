using System.Numerics;
using BepuPhysics;

namespace scpcb.Graphics; 

public class PhysicsModel : Model {
    private readonly BodyReference _body;

    public PhysicsModel(BodyReference body, params ICBMesh[] meshes) : base(meshes) {
        _body = body;
    }

    public override Transform WorldTransform {
        get => new(_body.Pose.Position, _body.Pose.Orientation);
        set {
            if (value.Scale != Vector3.One) {
                throw new ArgumentException("Scale must be 1", nameof(value));
            }
            _body.Pose = new(value.Position, value.Rotation);
        }
    }
}
