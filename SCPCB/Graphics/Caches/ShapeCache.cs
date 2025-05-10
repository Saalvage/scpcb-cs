using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Shapes;

namespace SCPCB.Graphics.Caches;

public class ShapeCache : BaseCache<(Type, Type), ICBMesh> {
    private readonly GraphicsResources _gfxRes;
    
    public ShapeCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBMesh<VPositionTexture> GetMesh<TShape>() where TShape : IShape {
        var key = (typeof(TShape), typeof(VPositionTexture));
        if (_dic.TryGetValue(key, out var mesh)) {
            return (ICBMesh<VPositionTexture>)mesh;
        }

        // TODO: Support other vertex types. I would like this to use a common interface with AssimpVertex, but its spans make that difficult.
        var ret = new CBMesh<VPositionTexture>(_gfxRes.GraphicsDevice, TShape.Vertices, TShape.Indices);
        _dic.Add(key, ret);
        return ret;
    }
}
