using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;

namespace SCPCB.Graphics.Caches;

// TODO: The design with the lambda here isn't ideal.
public class ModelCache(GraphicsResources gfxRes, PhysicsResources physics, Func<string, IModelLoader> loader) : BaseCache<string, OwningPhysicsModelTemplate> {
    public IPhysicsModelTemplate GetModel(string file)
        => _dic.TryGetValue(file, out var val)
            ? val
            : _dic[file] = loader(file).LoadModelWithCollision(gfxRes.GraphicsDevice, physics);
}
