using BepuPhysics;
using BepuPhysics.Collidables;
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

    private readonly Dictionary<Type, object> _collidableProperties = [];

    public BufferPool BufferPool { get; } = new();
    
    // TODO: This doesn't really belong here..
    public ModelCache ModelCache { get; }

    private readonly ThreadDispatcher _threadDispatcher;

    public event Action BeforeUpdate;
    public event Action AfterUpdate;

    public PhysicsResources(GraphicsResources gfxRes) {
        ModelCache = new(gfxRes, this, new AutomaticAssimpModelLoader<ModelShader, VPositionTexture, GraphicsResources>(gfxRes));

        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new(4, 2));

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new(targetThreadCount);
    }

    public void SetProperty<T, TVal>(CollidableReference handle, TVal val) where TVal : unmanaged {
        CollidableProperty<TVal> prop;
        if (_collidableProperties.TryGetValue(typeof(T), out var raw)) {
            prop = (CollidableProperty<TVal>)raw;
        } else {
            prop = new(Simulation);
            _collidableProperties.Add(typeof(T), prop);
        }
        // Allocate is unfortunate naming, it just makes sure we're not having a buffer overrun
        // because not every collidable will have a visibility property.
        prop.Allocate(handle) = val;
    }

    public TVal GetProperty<T, TVal>(CollidableReference handle) where TVal : unmanaged {
        if (_collidableProperties.TryGetValue(typeof(T), out var raw)) {
            return ((CollidableProperty<TVal>)raw).Allocate(handle);
        }
        return default;
    }

    public void Tick() {
        BeforeUpdate?.Invoke();
        // TODO: This stinks, allow for dynamic deltas.
        Simulation.Timestep(Game.TICK_DELTA, _threadDispatcher);
        AfterUpdate?.Invoke();
    }

    protected override void DisposeImpl() {
        ModelCache.Dispose();
        _threadDispatcher.Dispose();
        Simulation.Dispose();
        BufferPool.Clear();
    }
}
