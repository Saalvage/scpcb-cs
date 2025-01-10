using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBStatic : CBCollidable {
    private StaticReference _reference;

    private StaticDescription _desc;

    public override RigidPose Pose {
        get => _reference.Pose;
        set => _reference.Pose = value;
    }

    public CBStatic(ICBShape shape, in StaticDescription desc) : base(shape.Physics, shape) {
        _desc = desc;
        Attach();
    }

    protected override void AttachImpl() {
        _reference = new(Physics.Simulation.Statics.Add(_desc), Physics.Simulation.Statics);
    }

    protected override void DetachImpl() {
        _desc = _desc with {
            Pose = _reference.Pose,
        };
        Physics.Simulation.Statics.Remove(_reference.Handle);
        _reference = default;
    }

    protected override CollidableReference GetCollidableReference() => new(_reference.Handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility == CollidableMobility.Static && other.StaticHandle == _reference.Handle;
}
