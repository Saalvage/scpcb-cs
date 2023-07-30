using System.Numerics;
using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Shaders;

namespace scpcb.Physics;

public class PhysicsModel : InterpolatedModel {
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
