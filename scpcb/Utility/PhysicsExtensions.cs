using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuPhysics;
using System.Numerics;
using scpcb.Physics;

namespace scpcb.Utility; 

public static class PhysicsExtensions {
    private struct VisibilityRayHitHandler : IRayHitHandler {
        public bool Hit;

        private readonly PhysicsResources _physics;

        public VisibilityRayHitHandler(PhysicsResources physics) {
            _physics = physics;
        }

        public bool AllowTest(CollidableReference collidable) {
            // Allocate is unfortunate naming, it just makes sure we're not having a buffer overrun
            // because not every collidable will have a visibility property.
            return !Hit && !_physics.Visibility.Allocate(collidable).IsInvisible;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex) => AllowTest(collidable);

        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
            Hit = true;
        }
    }

    public static bool RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 to) {
        var dir = to - from;
        return physics.RayCastVisible(from, dir, 1f);
    }

    public static bool RayCastVisible(this PhysicsResources physics, Vector3 from, Vector3 dir, float length) {
        VisibilityRayHitHandler handler = new(physics);
        physics.Simulation.RayCast(from, dir, length, ref handler);
        return handler.Hit;
    }
}
