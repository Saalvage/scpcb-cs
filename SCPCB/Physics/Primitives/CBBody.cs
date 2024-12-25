using BepuPhysics;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public class CBBody : Disposable {
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

    public CBBody(Simulation sim, ICBShape shape, in BodyDescription desc) {
        _shape = shape;
        _bodies = sim.Bodies;
        _desc = desc;
        Attach();
    }

    public void Attach() {
        if (Attached) {
            return;
        }

        _reference = new(_bodies.Add(_desc), _bodies);
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
        _bodies.Remove(_reference.Handle);
        _reference = default;
        Attached = false;
    }

    protected override void DisposeImpl() {
        Detach();
    }
}
