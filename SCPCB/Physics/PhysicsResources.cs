using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.Assimp;
using SCPCB.Graphics.Caches;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Utility;

namespace SCPCB.Physics;

public class PhysicsResources : Disposable, ITickable {
    public Simulation Simulation { get; }

    public CollidableProperty<Visibility> Visibility { get; }

    public BufferPool BufferPool { get; } = new();
    
    // TODO: This doesn't really belong here..
    public ModelCache ModelCache { get; }

    private readonly ThreadDispatcher _threadDispatcher;

    public event Action BeforeUpdate;
    public event Action AfterUpdate;

    public PhysicsResources(GraphicsResources gfxRes) {
        ModelCache = new(gfxRes, this, new AutomaticAssimpModelLoader<ModelShader, VPositionTexture, GraphicsResources>(gfxRes));

        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new(4, 2));
        Visibility = new(Simulation);

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new(targetThreadCount);
    }

    public void Tick() {
        BeforeUpdate?.Invoke();
        // TODO: This stinks, allow for dynamic deltas.
        Simulation.Timestep(1f / Game.TICK_RATE, _threadDispatcher);
        AfterUpdate?.Invoke();
    }

    protected override void DisposeImpl() {
        ModelCache.Dispose();
        _threadDispatcher.Dispose();
        Simulation.Dispose();
        BufferPool.Clear();
    }
}
