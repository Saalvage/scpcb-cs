using System.Numerics;
using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Shaders;

namespace scpcb.Physics;

public class PhysicsModel : InterpolatedModel {
    private readonly PhysicsResources _physics;
    private readonly BodyReference _body;

    public PhysicsModel(PhysicsResources physics, BodyReference body, params ICBMesh[] meshes) : base(meshes) {
        _physics = physics;
        _body = body;
        physics.AfterUpdate += UpdateTransform;
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

    ~PhysicsModel() {
        _physics.AfterUpdate -= UpdateTransform; // TODO: Implement IDisposable?
    }
}
