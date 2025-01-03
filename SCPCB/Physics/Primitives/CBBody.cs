using BepuPhysics;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public class CBBody : Disposable {
    private readonly PhysicsResources _physics;

    private readonly ICBShape _shape;
    private readonly Bodies _bodies;
    
    private BodyDescription _desc;
    public BodyReference _reference;
    public bool Attached { get; private set; }

    public RigidPose Pose {
        get => _reference.Pose;
        set => _reference.Pose = value;
    }

    public BodyVelocity Velocity {
        get => _reference.Velocity;
        set => _reference.Velocity = value;
    }

    private bool _isInvisible;
    public bool IsInvisible {
        get => _isInvisible;
        set {
            _isInvisible = value;
            _physics.Visibility.Allocate(_reference).IsInvisible = value;
        }
    }

    public CBBody(PhysicsResources physics, ICBShape shape, in BodyDescription desc) {
        _physics = physics;
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
        IsInvisible = _isInvisible;
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
        _physics.Visibility[_reference].IsInvisible = false;
        _bodies.Remove(_reference.Handle);
        _reference = default;
        Attached = false;
    }

    protected override void DisposeImpl() {
        Detach();
    }
}
