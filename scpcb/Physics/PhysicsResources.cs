using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using scpcb.Entities;
using scpcb.Utility;

namespace scpcb.Physics;

public class PhysicsResources : Disposable, ITickable {
    public Simulation Simulation { get; }

    public BufferPool BufferPool { get; } = new();
    
    private readonly ThreadDispatcher _threadDispatcher;

    public event Action BeforeUpdate;
    public event Action AfterUpdate;

    public PhysicsResources() {
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new(4, 2));

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new(targetThreadCount);

        Simulation.Statics.Add(new(new Vector3(0, -0.5f, 0), Simulation.Shapes.Add(new Box(2500, 1, 2500))));
    }

    public void Tick() {
        BeforeUpdate?.Invoke();
        // TODO: This stinks, allow for dynamic deltas.
        Simulation.Timestep(1f / Game.TICK_RATE, _threadDispatcher);
        AfterUpdate?.Invoke();
    }

    protected override void DisposeImpl() {
        _threadDispatcher.Dispose();
        Simulation.Dispose();
        BufferPool.Clear();
    }
}
