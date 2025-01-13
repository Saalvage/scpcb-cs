using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
using SCPCB.Physics.Primitives;

namespace SCPCB.Physics;

public static class CBShapeExtensions {
    // These methods may appear redundant, however, I believe they provide a natural and intuitive way
    // to proceed when receiving/creating an ICBShape, without requiring concrete knowledge about
    // CBBody and CBStatic and prevent the question "what now?" from arising.
    public static CBConvexBody<T> CreateDynamic<T>(this ICBShape<T> shape, float mass)
        where T : unmanaged, IConvexShape
        => new(shape, mass);

    public static CBConvexBody<T> CreateKinematic<T>(this ICBShape<T> shape)
        where T : unmanaged, IConvexShape
        => new(shape) { IsKinematic = true };

    public static CBStatic CreateStatic(this ICBShape shape)
        => new(shape);

    public static CBStatic CreateStatic(this ICBShape shape, RigidPose pose)
        => new(shape, pose);

    // TODO: I think these designs sucks, it's literally an INTERFACE.
    public static BodyActivityDescription GetDefaultActivity<T>(this ICBShape<T> shape) where T : unmanaged, IConvexShape
        => BodyDescription.GetDefaultActivity(shape.Shape);

    public static BodyInertia ComputeInertia<T>(this ICBShape<T> shape, float mass) where T : unmanaged, IConvexShape
        => shape.Shape.ComputeInertia(mass);

    public static void ComputeBounds(this ICBShape shape, Quaternion rotation, out Vector3 min, out Vector3 max) {
        // TODO: Does this box the shape? Ideally it shouldn't.
        switch (shape.Shape) {
            case IConvexShape cs:
                cs.ComputeBounds(rotation, out min, out max);
                break;
            default:
                throw new NotSupportedException($"Computing bounds for {shape.GetType()} is not currently supported!");
        }
    }
}
