using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
using BepuUtilities;
using scpcb.Physics.Primitives;

namespace scpcb.Physics;

public static class CBShapeExtensions {
    public static CBBody CreateDynamic(this ICBShape shape, RigidPose pose, BodyInertia inertia, BodyActivityDescription activity)
        => new(shape.Simulation, shape, BodyDescription.CreateDynamic(pose, inertia, new(shape.ShapeIndex), activity));

    public static CBBody CreateKinematic(this ICBShape shape, RigidPose pose, BodyActivityDescription activity)
        => new(shape.Simulation, shape, BodyDescription.CreateKinematic(pose, new(shape.ShapeIndex), activity));

    public static CBStatic CreateStatic(this ICBShape shape, RigidPose pose)
        => new(shape.Simulation, shape, new(pose, shape.ShapeIndex));

    public static CBBody CreateDynamic<T>(this ICBShape<T> shape, RigidPose pose, float mass)
            where T : unmanaged, IConvexShape
        => new(shape.Simulation, shape,
            new() {
                Pose = pose,
                Activity = BodyDescription.GetDefaultActivity(shape.Shape),
                Collidable = shape.ShapeIndex,
                LocalInertia = shape.Shape.ComputeInertia(mass),
            });

    public static CBBody CreateKinematic<T>(this ICBShape<T> shape, RigidPose pose)
            where T : unmanaged, IConvexShape
        => new(shape.Simulation, shape, new() {
            Pose = pose,
            Activity = BodyDescription.GetDefaultActivity(shape.Shape),
            Collidable = shape.ShapeIndex,
        });

    public static ICBShape<ConvexHull> CreateScaledCopy(this ICBShape<ConvexHull> shape, Vector3 scale) {
        Matrix3x3.CreateScale(scale, out var transform);
        return shape.CreateTransformedCopy(transform);
    }

    public static ICBShape<ConvexHull> CreateTransformedCopy(this ICBShape<ConvexHull> shape, in Matrix3x3 transform) {
        ConvexHullHelper.CreateTransformedCopy(shape.Shape, transform, shape.Simulation.BufferPool, out var scaledHull);
        return new CBShape<ConvexHull>(shape.Simulation, scaledHull);
    }
}
