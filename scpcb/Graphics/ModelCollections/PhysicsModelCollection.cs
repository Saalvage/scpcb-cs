using System.Numerics;
using BepuPhysics;
using scpcb.Graphics.Primitives;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Graphics.ModelCollections;

public sealed class PhysicsModelCollection : InterpolatedModelCollection {
    private readonly PhysicsResources _physics;
    private readonly BodyReference _body;

    public PhysicsModelCollection(PhysicsResources physics, BodyReference body, IReadOnlyList<ICBModel> models) : base(models) {
        _physics = physics;
        _body = body;
        physics.AfterUpdate += UpdateTransform;
        Teleport(WorldTransform);
    }

    private Vector3 _scale = Vector3.One;

    public override Transform WorldTransform {
        get => _body.Pose.ToTransform() with { Scale = _scale };
        set {
            if (value.Scale != Vector3.One) {
                // TODO: Deal with this correctly.
                //throw new ArgumentException("Scale must be 1", nameof(value));
            }
            _body.Pose = new(value.Position, value.Rotation);
            _scale = value.Scale;
        }
    }

    ~PhysicsModelCollection() {
        _physics.AfterUpdate -= UpdateTransform; // TODO: Implement IDisposable?
    }
}
