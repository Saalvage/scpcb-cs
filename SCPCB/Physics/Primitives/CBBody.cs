using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBBody : CBCollidable {
    private readonly ICBShape _shape;
    private readonly Bodies _bodies;
    
    private BodyDescription _desc;
    private BodyReference _reference;
    public bool Attached { get; private set; }

    public RigidPose Pose {
        get => _reference.Pose;
        set => _reference.Pose = value;
    }

    public BodyVelocity Velocity {
        get => _reference.Velocity;
        set => _reference.Velocity = value;
    }

    public CBBody(PhysicsResources physics, ICBShape shape, in BodyDescription desc) : base(physics) {
        _shape = shape;
        _bodies = physics.Simulation.Bodies;
        _desc = desc;
        Attach();
    }

    public void Attach() {
        if (Attached) {
            return;
        }

        _reference = new(_bodies.Add(_desc), _bodies);
        // We need to set it again because we might have received a different reference.
        ReapplyProperties();
        Attached = true;
    }

    public void Detach() {
        if (!Attached) {
            return;
        }

        _desc = _desc with {
            LocalInertia = _reference.LocalInertia,
            Pose = _reference.Pose,
            Velocity = _reference.Velocity,
        };
        // TODO: Can handles be reassigned to a different entity? This could be cause for some nasty bugs if it were the case.
        // We know that detaching and reattaching requires resetting of properties to make sure they're correct.
        // If they CANNOT, then this is unnecessary.
        ResetProperties();
        _bodies.Remove(_reference.Handle);
        _reference = default;
        Attached = false;
    }

    protected override void DisposeImpl() {
        Detach();
    }

    protected override CollidableReference GetCollidableReference() => new(CollidableMobility.Dynamic, _reference.Handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility != CollidableMobility.Static && other.BodyHandle == _reference.Handle;
}
