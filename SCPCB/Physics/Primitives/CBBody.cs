using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public class CBBody : CBCollidable {
    protected BodyDescription _desc;
    protected BodyReference _reference;

    public BodyInertia Inertia {
        get => _desc.LocalInertia;
        set {
            _desc.LocalInertia = value;
            if (IsAttached) {
                _reference.SetLocalInertia(value);
            }
        }
    }

    public BodyActivityDescription Activity {
        get => _desc.Activity;
        set {
            _desc.Activity = value;
            if (IsAttached) {
                _reference.Activity.SleepThreshold = value.SleepThreshold;
                _reference.Activity.MinimumTimestepsUnderThreshold = value.MinimumTimestepCountUnderThreshold;
                if (value.SleepThreshold < 0) {
                    _reference.Awake = true;
                }
            }
        }
    }

    public override RigidPose Pose {
        get => IsAttached ? _reference.Pose : _desc.Pose;
        set {
            if (IsAttached) {
                _reference.Awake = true;
                _reference.Pose = value;
            } else {
                _desc.Pose = value;
            }
        }
    }

    public BodyVelocity Velocity {
        get => IsAttached ? _reference.Velocity : _desc.Velocity;
        set {
            if (IsAttached) {
                _reference.Awake = true;
                _reference.Velocity = value;
            } else {
                _desc.Velocity = value;
            }
        }
    }

    public CBBody(ICBShape shape, in BodyDescription desc) : base(shape.Physics, shape) {
        _desc = desc;
        Attach();
    }

    protected override void UpdateShape(ICBShape newShape) {
        _desc.Collidable = newShape.ShapeIndex;
        if (IsAttached) {
            _reference.SetShape(newShape.ShapeIndex);
        }
    }

    protected override void AttachImpl() {
        _reference = new(Physics.Simulation.Bodies.Add(_desc), Physics.Simulation.Bodies);
    }

    protected override void DetachImpl() {
        _desc = _desc with {
            Pose = _reference.Pose,
            Velocity = _reference.Velocity,
        };
        Physics.Simulation.Bodies.Remove(_reference.Handle);
        _reference = default;
    }

    protected override CollidableReference GetCollidableReference()
        => new(CollidableMobility.Dynamic, _reference.Handle);

    public override bool Equals(CollidableReference other)
        => other.Mobility != CollidableMobility.Static && other.BodyHandle == _reference.Handle;
}
