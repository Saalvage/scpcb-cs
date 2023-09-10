using BepuPhysics;
using scpcb.Utility;

namespace scpcb.Physics.Primitives;

public class CBBody : Disposable {
    private readonly ICBShape _shape;

    private BodyReference Reference { get; }

    public RigidPose Pose {
        get => Reference.Pose;
        set => Reference.Pose = value;
    }

    public BodyVelocity Velocity {
        get => Reference.Velocity;
        set => Reference.Velocity = value;
    }

    public CBBody(Simulation sim, ICBShape shape, in BodyDescription desc) {
        _shape = shape;
        Reference = new(sim.Bodies.Add(desc), sim.Bodies);
    }

    protected override void DisposeImpl() {
        Reference.Bodies.Remove(Reference.Handle);
    }
}
