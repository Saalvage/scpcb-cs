using SCPCB.Graphics.Primitives;

namespace SCPCB.Graphics.Caches; 

public class MeshCache : BaseCache<Type, ICBMesh> {
    private readonly GraphicsResources _gfxRes;

    public MeshCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBMesh GetMesh<T>() where T : ISharedMeshProvider<T>
        => _dic.TryGetValue(typeof(T), out var val) ?
            val : _dic[typeof(T)] = T.CreateSharedMesh(_gfxRes);

    public ICBMesh<TVertex> GetMesh<T, TVertex>() where TVertex : unmanaged where T : ISharedMeshProvider<T, TVertex>
        => (ICBMesh<TVertex>)(_dic.TryGetValue(typeof(T), out var val) ?
            val : _dic[typeof(T)] = T.CreateSharedMesh(_gfxRes));
}
