using SCPCB.Graphics.Animation;
using SCPCB.Graphics.Shaders;

namespace SCPCB.Graphics.Caches;

public class AnimatedModelCache : BaseCache<string, OwningAnimatedModelTemplate> {
    private readonly GraphicsResources _gfxRes;
    
    public AnimatedModelCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public IAnimatedModelTemplate GetAnimatedModel(string file) {
        if (_dic.TryGetValue(file, out var model)) {
            return model;
        }

        var ret = new AssimpAnimatedModelLoader<AnimatedModelShader, AnimatedModelShader.Vertex,
                GraphicsResources>(_gfxRes, file)
            .LoadAnimatedModel(_gfxRes.GraphicsDevice);
        _dic.Add(file, ret);
        return ret;
    }
}
