using System.Numerics;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using SCPCB.Physics;
using SCPCB.Utility;

namespace SCPCB.Graphics;

public class ModelImageGenerator {
    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;

    private readonly RenderTexture _texture;
    public ICBTexture Texture => _texture;

    public Transform Transform { get; set; }

    private string _prevMeshFile;
    public string MeshFile { get; set; }

    private ICBModelTemplate _template;
    private Model _model;

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
            _template = _physics.ModelCache.GetModel(MeshFile);
            _model = _template.Instantiate();
        }

        _gfxRes.ShaderCache.SetGlobal<IViewMatrixConstantMember, Matrix4x4>(_cam.GetViewMatrix(1));
        _gfxRes.ShaderCache.SetGlobal<IViewPositionConstantMember, Vector3>(_cam.Position);

        _texture.Start();

        _model.WorldTransform = Transform;
        foreach (var model in _model.Models) {
            model.MeshInstance.Render(_texture, 0f);
        }
        _texture.End();

        return true;
    }

    public Matrix4x4 GetValue(float interp) => Transform.GetMatrix();
}
