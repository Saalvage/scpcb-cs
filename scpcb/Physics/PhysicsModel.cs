using System.Numerics;
using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Shaders;

namespace scpcb.Physics;

public sealed class PhysicsModel : InterpolatedModel {
    private readonly PhysicsResources _physics;
    private readonly BodyReference _body;

    public PhysicsModel(PhysicsResources physics, BodyReference body, params ICBMesh[] meshes) : base(meshes) {
        _physics = physics;
        _body = body;
        physics.AfterUpdate += UpdateTransform;
        Teleport(WorldTransform);
    }

    public override Transform WorldTransform {
        get => _body.Pose.ToTransform();
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
