using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBStatic : CBCollidable {
    private readonly ICBShape _shape;
    private readonly StaticHandle _handle;
    private readonly StaticReference _ref;

    public RigidPose Pose {
        get => _ref.Pose;
        set => _ref.Pose = value;
    }

    public CBStatic(PhysicsResources physics, ICBShape shape, in StaticDescription desc) : base(physics) {
        _shape = shape;
        _handle = _physics.Simulation.Statics.Add(desc);
        _ref = _physics.Simulation.Statics[_handle];
    }

    protected override void DisposeImpl() {
        _physics.Simulation.Statics.Remove(_handle);
    }

    protected override CollidableReference GetCollidableReference() => new(_handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility == CollidableMobility.Static && other.StaticHandle == _handle;
}
