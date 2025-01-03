using BepuPhysics;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public class CBStatic : Disposable {
    private readonly PhysicsResources _physics;
    private readonly ICBShape _shape;
    private readonly StaticHandle _handle;
    private readonly StaticReference _ref;

    public RigidPose Pose {
        get => _ref.Pose;
        set => _ref.Pose = value;
    }

    private bool _isInvisible;
    public bool IsInvisible {
        get => _isInvisible;
        set {
            // Remember to make sure to (re)apply this when (re)attaching statics becomes possible later on.
            _isInvisible = value;
            _physics.Visibility.Allocate(_handle).IsInvisible = value;
        }
    }

    public CBStatic(PhysicsResources physics, ICBShape shape, in StaticDescription desc) {
        _physics = physics;
        _shape = shape;
        _handle = _physics.Simulation.Statics.Add(desc);
        _ref = _physics.Simulation.Statics[_handle];
    }

    protected override void DisposeImpl() {
        _physics.Simulation.Statics.Remove(_handle);
    }
}
