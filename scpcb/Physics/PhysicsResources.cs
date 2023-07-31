using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace scpcb.Physics;

public class PhysicsResources : Disposable {
    public Simulation Simulation { get; }

    public BufferPool BufferPool { get; } = new();
    
    private readonly ThreadDispatcher ThreadDispatcher;

    // TODO vvv Remove vvv
    public List<BodyReference> Bodies { get; } = new();
    // TODO ^^^ Remove ^^^

    public PhysicsResources() {
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new(4, 2));

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        ThreadDispatcher = new(targetThreadCount);

        Simulation.Statics.Add(new(new Vector3(0, -0.5f, 0), Simulation.Shapes.Add(new Box(2500, 1, 2500))));
    }

    public void Update(float delta) {
        Simulation.Timestep(delta, ThreadDispatcher);
    }

    protected override void DisposeImpl() {
        ThreadDispatcher.Dispose();
        Simulation.Dispose();
        BufferPool.Clear();
    }
}
