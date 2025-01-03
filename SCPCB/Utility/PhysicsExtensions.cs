using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuPhysics;
using System.Numerics;
using SCPCB.Physics;

namespace SCPCB.Utility; 

public static class PhysicsExtensions {
    private struct RayHitHandler : IRayHitHandler {
        public CollidableReference? Hit { get; private set; }

        private readonly Predicate<CollidableReference> _pred;

        public RayHitHandler(Predicate<CollidableReference> pred) {
            _pred = pred;
        }

        public bool AllowTest(CollidableReference collidable) {
            // Allocate is unfortunate naming, it just makes sure we're not having a buffer overrun
            // because not every collidable will have a visibility property.
            return Hit is null && _pred(collidable);
        }

        public bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);

        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
            Hit = collidable;
        }
    }

    public static CollidableReference? RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 to) {
        var dir = to - from;
        return physics.RayCastVisible(from, dir, 1f);
    }

    public static CollidableReference? RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 dir, float length) {
        // Allocate is unfortunate naming, it just makes sure we're not having a buffer overrun
        // because not every collidable will have a visibility property.
        RayHitHandler handler = new(x => !physics.GetProperty<Visibility, bool>(x));
        physics.Simulation.RayCast(from, dir, length, ref handler);
        return handler.Hit;
    }
}
