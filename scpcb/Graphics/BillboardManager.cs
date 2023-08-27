using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Utility;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics; 

// TODO: We NEED a better mechanism for sharing meshes..
// We can't just create a giant ass class we need to drag around everywhere just to share a singular mesh!
public class BillboardManager : Disposable {
    private readonly GraphicsResources _gfxRes;

    private readonly ICBMesh<BillboardShader.Vertex> _mesh;
    private readonly WeakDictionary<ICBTexture, ICBMaterial<BillboardShader.Vertex>> _materialCache = new();

    public BillboardManager(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;

        _mesh = new CBMesh<BillboardShader.Vertex>(gfxRes.GraphicsDevice, new BillboardShader.Vertex[] {
                new(new(-1f, 1f, 0), new(1, 0)),
                new(new(1f, 1f, 0), new(0, 0)),
                new(new(-1f, -1f, 0), new(1, 1)),
                new(new(1f, -1f, 0), new(0, 1)),
            },
            new uint[] {0, 1, 2, 3, 2, 1});
    }

    public Billboard Create(ICBTexture texture, bool shouldRenderOnTop = false) {
        if (!_materialCache.TryGetValue(texture, out var mat)) {
            mat = _gfxRes.ShaderCache.GetShader<BillboardShader, BillboardShader.Vertex>(shouldRenderOnTop
                    ? x => x with { DepthState = DepthStencilStateDescription.Disabled } : null)
                .CreateMaterial(texture.AsEnumerableElement(), _gfxRes.ClampAnisoSampler.AsEnumerableElement());
            _materialCache.Add(texture, mat);
        }

        return new(_mesh, mat);
    }

    protected override void DisposeImpl() {
        _mesh.Dispose();
    }
}
