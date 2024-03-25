using System.Numerics;
using BepuPhysics;
using scpcb.Utility;

namespace scpcb.Physics.Primitives;

public class CBStatic : Disposable {
    private readonly Simulation _sim;
    private readonly ICBShape _shape;
    private readonly StaticHandle _handle;
    private readonly StaticReference _ref;

    public RigidPose Pose {
        get => _ref.Pose;
        set => _ref.Pose = value;
    }

    public CBStatic(Simulation sim, ICBShape shape, in StaticDescription desc) {
        _sim = sim;
        _shape = shape;
        _handle = sim.Statics.Add(desc);
        _ref = _sim.Statics[_handle];
    }

    protected override void DisposeImpl() {
        _sim.Statics.Remove(_handle);
    }
}
