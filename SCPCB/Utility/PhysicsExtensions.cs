using System.Diagnostics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuPhysics;
using System.Numerics;
using SCPCB.Physics;

namespace SCPCB.Utility;

public record struct CollisionResult(CollidableReference Hit, Vector3 Pos, Vector3 Normal);

public interface ICustomizableRayHitHandler<T> : IRayHitHandler where T : ICustomizableRayHitHandler<T> {
    public CollisionResult? Result { get; }
    static abstract T Create(Predicate<CollidableReference> pred);
}

public struct AnyRayHitHandler : ICustomizableRayHitHandler<AnyRayHitHandler> {
    public CollisionResult? Result { get; private set; }

    private readonly Predicate<CollidableReference> _pred;

    public AnyRayHitHandler(Predicate<CollidableReference> pred) {
        _pred = pred;
    }

    public static AnyRayHitHandler Create(Predicate<CollidableReference> pred) => new(pred);

    public bool AllowTest(CollidableReference collidable) => !Result.HasValue && _pred(collidable);

    public bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);

    public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
        Result = new(collidable, ray.Origin + ray.Direction * t, normal);
    }
}

public struct ClosestRayHitHandler : ICustomizableRayHitHandler<ClosestRayHitHandler> {
    public CollisionResult? Result { get; private set; }

    private readonly Predicate<CollidableReference> _pred;

    public ClosestRayHitHandler(Predicate<CollidableReference> pred) {
        _pred = pred;
    }

    public static ClosestRayHitHandler Create(Predicate<CollidableReference> pred) => new(pred);

    public bool AllowTest(CollidableReference collidable) => _pred(collidable);

    public bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);

    public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
        Debug.Assert(t <= maximumT);
        Result = new(collidable, ray.Origin + ray.Direction * t, normal);
        maximumT = t;
    }
}

public static class PhysicsExtensions {
    public static CollisionResult? RayCast<T>(this PhysicsResources physics, Vector3 from, Vector3 to) where T : ICustomizableRayHitHandler<T> {
        var dir = to - from;
        return physics.RayCast<T>(from, dir, 1f);
    }

    public static CollisionResult? RayCast<T>(this PhysicsResources physics, Vector3 from, Vector3 dir, float length, Predicate<CollidableReference>? pred = null) where T : ICustomizableRayHitHandler<T> {
        var handler = T.Create(pred ?? (_ => true));
        physics.Simulation.RayCast(from, dir, length, ref handler);
        return handler.Result;
    }

    public static CollisionResult? RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 to) {
        var dir = to - from;
        return physics.RayCastVisible(from, dir, 1f);
    }

    public static CollisionResult? RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 dir, float length)
        => RayCast<AnyRayHitHandler>(physics, from, dir, length, x => !physics.GetProperty<IsInvisibleProperty, bool>(x));
}
