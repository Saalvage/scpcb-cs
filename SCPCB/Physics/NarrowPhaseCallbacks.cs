using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace SCPCB.Physics;

public readonly struct NarrowPhaseCallbacks : INarrowPhaseCallbacks {
    private readonly PhysicsResources _physics;

    public SpringSettings ContactSpringiness { get; } = new(30, 1);
    public float MaximumRecoveryVelocity { get; } = 2;
    public float FrictionCoefficient { get; } = 0.7f;

    public NarrowPhaseCallbacks(PhysicsResources physics) {
        _physics = physics;
    }

    public void Initialize(Simulation simulation) {
        // We can't assert that it's the same Simulation as in the PhysicsResources here
        // because this gets called in the Simulation's ctor.
    }

    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        => a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold,
        out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold> {
        pairMaterial.FrictionCoefficient = _physics.GetProperty<HasNoFrictionProperty, bool>(pair.A)
                                           || _physics.GetProperty<HasNoFrictionProperty, bool>(pair.B)
            ? 0 : FrictionCoefficient;
        pairMaterial.MaximumRecoveryVelocity = MaximumRecoveryVelocity;
        pairMaterial.SpringSettings = ContactSpringiness;
        return true;
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        => true;

    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB,
        ref ConvexContactManifold manifold) => true;

    public void Dispose() { }
}
