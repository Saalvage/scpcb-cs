using System.Numerics;
using BepuPhysics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Graphics.ModelCollections;

public sealed class PhysicsModelCollection : InterpolatedModelCollection, IEntity {
    private readonly PhysicsResources _physics;

    public BodyReference Body { get; }

    public PhysicsModelCollection(PhysicsResources physics, BodyReference body, IReadOnlyList<ICBModel> models) : base(models) {
        _physics = physics;
        Body = body;
        physics.AfterUpdate += UpdateTransform;
        Teleport(WorldTransform);
    }

    private Vector3 _scale = Vector3.One;

    public override Transform WorldTransform {
        get => Body.Pose.ToTransform() with { Scale = _scale };
        set {
            if (value.Scale != Vector3.One) {
                // TODO: Deal with this correctly.
                //throw new ArgumentException("Scale must be 1", nameof(value));
            }
            Body.Pose = new(value.Position, value.Rotation);
            _scale = value.Scale;
        }
    }

    void IEntity.OnRemove(IScene scene) {
        _physics.Simulation.Bodies.Remove(Body.Handle);
        _physics.AfterUpdate -= UpdateTransform;
    }
}
