using BepuPhysics;
using BepuPhysics.Collidables;

namespace SCPCB.Physics.Primitives;

public abstract class CBConvexBody : CBBody {
    public abstract float Mass { get; set; }
    public abstract bool IsKinematic { get; set; }
    public abstract bool MaySleep { get; set; }

    protected CBConvexBody(ICBShape shape, in BodyDescription desc) : base(shape, in desc) { }
}

public class CBConvexBody<T> : CBConvexBody where T : unmanaged, IConvexShape {
    public new ICBShape<T> Shape => (ICBShape<T>)base.Shape;

    private float _mass;
    public override float Mass {
        get => IsKinematic ? float.PositiveInfinity : _mass;
        set {
            if (IsKinematic) {
                throw new InvalidOperationException("Cannot set mass of a kinematic body!");
            }

            if (_mass == value) {
                return;
            }

            _mass = value;
            _desc.LocalInertia = Shape.ComputeInertia(value);
            if (IsAttached) {
                _reference.SetLocalInertia(_desc.LocalInertia);
            }
        }
    }

    public override bool IsKinematic {
        // TODO: Why don't these math types implement IEquatable?
        get => _desc.LocalInertia.Equals(default(BodyInertia));
        set {
            if (IsKinematic == value) {
                return;
            }

            if (value) {
                _desc.LocalInertia = default;
            } else {
                _desc.LocalInertia = Shape.ComputeInertia(_mass);
            }
            if (IsAttached) {
                _reference.SetLocalInertia(_desc.LocalInertia);
            }
        }
    }

    public override bool MaySleep {
        get => float.IsNegativeInfinity(_desc.Activity.SleepThreshold);
        set {
            if (MaySleep == value) {
                return;
            }

            if (value) {
                _desc.Activity = Shape.GetDefaultActivity();
                if (IsAttached) {
                    _reference.Activity.SleepThreshold = _desc.Activity.SleepThreshold;
                    _reference.Activity.MinimumTimestepsUnderThreshold = _desc.Activity.MinimumTimestepCountUnderThreshold;
                }
            } else {
                _desc.Activity.SleepThreshold = float.NegativeInfinity;
                if (IsAttached) {
                    _reference.Activity.SleepThreshold = float.NegativeInfinity;
                    _reference.Awake = true;
                }
            }
        }
    }

    public CBConvexBody(ICBShape<T> shape, float mass = 0) : base(shape, new() {
        Activity = shape.GetDefaultActivity(),
        Collidable = shape.ShapeIndex,
        LocalInertia = shape.ComputeInertia(mass),
        Pose = RigidPose.Identity,
        Velocity = default,
    }) {
        _mass = mass;
        Attach();
    }

    protected override void UpdateShape(ICBShape ns) {
        if (ns is not ICBShape<T> newShape) {
            throw new ArgumentException("Cannot set to non-convex shape!", nameof(newShape));
        }

        _desc.Collidable = newShape.ShapeIndex;
        var newActivity = newShape.GetDefaultActivity();
        _desc.Activity = newActivity;
        if (!IsKinematic) {
            var newInertia = newShape.ComputeInertia(_mass);
            _desc.LocalInertia = newInertia;
        }
        if (IsAttached) {
            _desc = _desc with {
                Pose = _reference.Pose,
                Velocity = _reference.Velocity,
            };
            _reference.ApplyDescription(_desc);
        }
    }
}
