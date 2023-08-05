using System.Numerics;
using BepuPhysics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Physics;

namespace scpcb.Graphics.ModelCollections;

public sealed class PhysicsModelCollection : InterpolatedModelCollection {
    private readonly PhysicsResources _physics;
    private readonly BodyReference _body;

    public PhysicsModelCollection(PhysicsResources physics, BodyReference body, params ICBModel[] models) : base(models) {
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

    ~PhysicsModelCollection() {
        _physics.AfterUpdate -= UpdateTransform; // TODO: Implement IDisposable?
    }
}
