using System.Numerics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Graphics.Textures;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Graphics;

public class ModelImageGenerator : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4> {
    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;

    private readonly RenderTexture _texture;
    public ICBTexture Texture => _texture;

    public Transform Transform { get; set; }

    private string _prevMeshFile;
    public string MeshFile { get; set; }

    private ModelCache.CacheEntry _cache;
    private ICBModel[] _models;

    private readonly PerspectiveCamera _cam = new() { Position = new(0, 0, -5) };

    public ModelImageGenerator(GraphicsResources gfxRes, PhysicsResources physics, uint width, uint height) {
        _gfxRes = gfxRes;
        _physics = physics;
        _texture = new(gfxRes, width, height);
    }

    public bool Update() {
        if (!File.Exists(MeshFile)) {
            return false;
        }

        if (_prevMeshFile != MeshFile) {
            _prevMeshFile = MeshFile;
            _cache = _physics.ModelCache.GetModel(MeshFile);
            _models = _cache.Models.Instantiate().ToArray();
            foreach (var m in _models) {
                m.ConstantProviders.Add(this);
            }
        }

        _gfxRes.ShaderCache.SetGlobal<IViewMatrixConstantMember, Matrix4x4>(_cam.ViewMatrix);
        _gfxRes.ShaderCache.SetGlobal<IViewPositionConstantMember, Vector3>(_cam.Position);

        _texture.Start();
        foreach (var model in _models) {
            model.Render(_texture, 0f);
        }
        _texture.End();

        return true;
    }

    public Matrix4x4 GetValue(float interp) => Transform.GetMatrix();
}
