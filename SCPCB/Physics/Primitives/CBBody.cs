using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBBody : CBCollidable {
    private BodyDescription _desc;
    private BodyReference _reference;

    public override RigidPose Pose {
        get => _reference.Pose;
        set => _reference.Pose = value;
    }

    public BodyVelocity Velocity {
        get => _reference.Velocity;
        set => _reference.Velocity = value;
    }

    public CBBody(ICBShape shape, in BodyDescription desc) : base(shape.Physics, shape) {
        _desc = desc;
        Attach();
    }

    protected override void AttachImpl() {
        _reference = new(Physics.Simulation.Bodies.Add(_desc), Physics.Simulation.Bodies);
    }

    protected override void DetachImpl() {
        _desc = _desc with {
            LocalInertia = _reference.LocalInertia,
            Pose = _reference.Pose,
            Velocity = _reference.Velocity,
        };
        Physics.Simulation.Bodies.Remove(_reference.Handle);
        _reference = default;
    }

    protected override CollidableReference GetCollidableReference() => new(CollidableMobility.Dynamic, _reference.Handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility != CollidableMobility.Static && other.BodyHandle == _reference.Handle;
}
