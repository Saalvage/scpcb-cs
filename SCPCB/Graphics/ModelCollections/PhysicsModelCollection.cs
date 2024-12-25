using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Primitives;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Graphics.ModelCollections;

public sealed class PhysicsModelCollection : InterpolatedModelCollection, IEntity {
    private readonly PhysicsResources _physics;

    public CBBody Body { get; }

    public PhysicsModelCollection(PhysicsResources physics, CBBody body, IReadOnlyList<ICBModel> models) : base(models) {
        _physics = physics;
        Body = body;
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

    void IEntity.OnAdd(IScene scene) {
        Body.Attach();
        _physics.AfterUpdate += UpdateTransform;
    }

    void IEntity.OnRemove(IScene scene) {
        _physics.AfterUpdate -= UpdateTransform;
        Body.Detach();
    }
}
