using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuPhysics;
using System.Numerics;

namespace scpcb.Utility; 

public static class PhysicsExtensions {
    private struct SingleRayHitHandler : IRayHitHandler {
        public bool Hit;

        public bool AllowTest(CollidableReference collidable) {
            return !Hit;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex) {
            return !Hit;
        }

        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
            Hit = true;
        }
    }

    public static bool SimpleRayCast(this Simulation sim, Vector3 from, Vector3 to) {
        var dir = to - from;
        return sim.SimpleRayCast(from, dir, dir.Length());
    }

    public static bool SimpleRayCast(this Simulation sim, Vector3 from, Vector3 dir, float length) {
        SingleRayHitHandler handler = default;
        sim.RayCast(from, dir, length, ref handler);
        return handler.Hit;
    }
}
