using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.Utility;

namespace SCPCB.Graphics.Caches;

public class ModelCache(GraphicsResources gfxRes, PhysicsResources physics, IModelLoader loader) : BaseCache<string, ModelCache.CacheEntry> {
    public class CacheEntry : Disposable {
        public IMeshMaterial[] Models { get; }
        public ICBShape<ConvexHull> Collision { get; }
        public Vector3 MiddleOffset { get; }

        public CacheEntry(IModelLoader converter, GraphicsResources gfxRes, PhysicsResources physics, string file) {
            Log.Information("Loading model {File} via {Converter}", file, converter);
            (Models, Collision, MiddleOffset) = converter.LoadMeshes(gfxRes.GraphicsDevice, physics, file);
        }

        protected override void DisposeImpl() {
            foreach (var m in Models) {
                m.Mesh.Dispose();
            }

            Collision.Dispose();
        }
    }

    public CacheEntry GetModel(string file)
        => _dic.TryGetValue(file, out var val) ? val
            : _dic[file] = new(loader, gfxRes, physics, file);
}
