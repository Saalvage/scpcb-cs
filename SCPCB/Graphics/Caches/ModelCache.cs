using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;

namespace SCPCB.Graphics.Caches;

public class ModelCache(GraphicsResources gfxRes, PhysicsResources physics, IModelLoader loader) : BaseCache<string, OwningPhysicsModelTemplate> {
    public IPhysicsModelTemplate GetModel(string file)
        => _dic.TryGetValue(file, out var val)
            ? val
            : _dic[file] = loader.LoadMeshesWithCollision(gfxRes.GraphicsDevice, physics, file);
}
