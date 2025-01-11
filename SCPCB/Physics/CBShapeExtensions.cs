using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
using BepuUtilities;
using SCPCB.Physics.Primitives;

namespace SCPCB.Physics;

public static class CBShapeExtensions {
    public static CBBody CreateDynamic(this ICBShape shape, RigidPose pose, BodyInertia inertia, BodyActivityDescription activity)
        => new(shape, BodyDescription.CreateDynamic(pose, inertia, new(shape.ShapeIndex), activity));

    public static CBBody CreateKinematic(this ICBShape shape, RigidPose pose, BodyActivityDescription activity)
        => new(shape, BodyDescription.CreateKinematic(pose, new(shape.ShapeIndex), activity));

    public static CBStatic CreateStatic(this ICBShape shape, RigidPose pose)
        => new(shape, new(pose, shape.ShapeIndex));

    public static CBBody CreateDynamic(this ICBShape shape, RigidPose pose, float mass)
        => new(shape,
            new() {
                Pose = pose,
                Activity = shape.GetDefaultActivity(),
                Collidable = shape.ShapeIndex,
                LocalInertia = shape.ComputeInertia(mass),
            });

    public static CBBody CreateKinematic(this ICBShape shape, RigidPose pose)
        => new(shape, new() {
            Pose = pose,
            Activity = shape.GetDefaultActivity(),
            Collidable = shape.ShapeIndex,
        });

    public static ICBShape CreateScaledCopy(this ICBShape shape, Vector3 scale) {
        Matrix3x3.CreateScale(scale, out var transform);
        return shape.CreateTransformedCopy(transform);
    }

    // TODO: I think these designs sucks, it's literally an INTERFACE.
    public static BodyActivityDescription GetDefaultActivity(this ICBShape shape) {
        switch (shape) {
            case ICBShape<ConvexHull> ch:
                return BodyDescription.GetDefaultActivity(ch.Shape);
            default:
                throw new NotSupportedException($"A default activity for {shape.GetType()} is not currently supported!");
        }
    }

    public static BodyInertia ComputeInertia(this ICBShape shape, float mass) {
        switch (shape) {
            case ICBShape<ConvexHull> ch:
                return ch.Shape.ComputeInertia(mass);
            default:
                throw new NotSupportedException($"Computing inertia for {shape.GetType()} is not currently supported!");
        }
    }

    public static ICBShape CreateTransformedCopy(this ICBShape shape, in Matrix3x3 transform) {
        switch (shape) {
            case ICBShape<ConvexHull> ch:
                ConvexHullHelper.CreateTransformedCopy(ch.Shape, transform, shape.Physics.BufferPool, out var scaledHull);
                return new CBShape<ConvexHull>(shape.Physics, scaledHull);
            default:
                throw new NotSupportedException($"Creating a transformed copy for {shape.GetType()} is not currently supported!");
        }
    }

    public static void ComputeBounds(this ICBShape shape, Quaternion rotation, out Vector3 min, out Vector3 max) {
        switch (shape) {
            case ICBShape<ConvexHull> ch:
                ch.Shape.ComputeBounds(rotation, out min, out max);
                break;
            default:
                throw new NotSupportedException($"Computing bounds for {shape.GetType()} is not currently supported!");
        }
    }
}
