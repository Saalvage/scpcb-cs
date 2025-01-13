using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBStatic : CBCollidable {
    private StaticDescription _desc;
    private StaticReference _reference;

    public override RigidPose Pose {
        get => IsAttached ? _reference.Pose : _desc.Pose;
        set {
            // The pose is as the type suggests, static.
            _desc.Pose = value;
            if (IsAttached) {
                _reference.Pose = value;
                // _reference.UpdateBounds();
                // TODO: It should be enough to update the bounds here..
                _reference.ApplyDescription(_desc);
            }
        }
    }

    public CBStatic(ICBShape shape) : this(shape, RigidPose.Identity) { }

    public CBStatic(ICBShape shape, RigidPose pose) : this(shape, pose, ContinuousDetection.Discrete) { }

    public CBStatic(ICBShape shape, RigidPose pose, ContinuousDetection continuity) : base(shape.Physics, shape) {
        _desc = new() {
            Continuity = continuity,
            Pose = pose,
            Shape = shape.ShapeIndex,
        };
        Attach();
    }

    protected override void UpdateShape(ICBShape newShape) {
        _desc.Shape = newShape.ShapeIndex;
        if (IsAttached) {
            _reference.SetShape(newShape.ShapeIndex);
        }
    }

    protected override void AttachImpl() {
        _reference = new(Physics.Simulation.Statics.Add(_desc), Physics.Simulation.Statics);
    }

    protected override void DetachImpl() {
        Physics.Simulation.Statics.Remove(_reference.Handle);
        _reference = default;
    }

    protected override CollidableReference GetCollidableReference() => new(_reference.Handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility == CollidableMobility.Static && other.StaticHandle == _reference.Handle;
}
