using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;
using SCPCB.Utility;

namespace SCPCB.Graphics.Caches;

// TODO: The design with the lambda here isn't ideal.
public class ModelCache(GraphicsResources gfxRes, PhysicsResources physics, Func<string, IModelLoader> loader) : Disposable {
    // TODO: Ehhh, I don't know how often we load both mesh and hull collision for the same model,
    // but we should probably handle these cases properly.
    private readonly WeakDictionary<string, OwningPhysicsModelTemplate> _modelsWithHullColl = [];
    private readonly WeakDictionary<string, OwningPhysicsModelTemplate> _modelsWithMeshColl = [];
    
    public IPhysicsModelTemplate GetModel(string file, bool hullColl = true) {
        var dic = hullColl ? _modelsWithHullColl : _modelsWithMeshColl;
        return dic.TryGetValue(file, out var val)
            ? val
            : dic[file] = loader(file).LoadModelWithCollision(gfxRes.GraphicsDevice, physics, hullColl);
    }

    protected override void DisposeImpl() {
        foreach (var template in _modelsWithHullColl.Select(x => x.Value)
                     .Concat(_modelsWithMeshColl.Select(x => x.Value))) {
            template.Dispose();
        }
    }
}
