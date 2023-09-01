using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Graphics.Textures;
using scpcb.Scenes;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics;

public class DebugLine : Disposable, IRenderable, IUpdatable, IConstantProvider<IColorConstantMember, Vector3> {
    private readonly BaseScene? _scene;

    private readonly ICBModel _model;

    public Vector3 Color { get; set; } = new(1, 0, 0);
    Vector3 IConstantProvider<IColorConstantMember, Vector3>.GetValue(float interp) => Color;

    private float _countDown;

    // TODO: The fact that we need to use a BaseScene suggests to me that we should consider moving the add/remove functions to IScene.
    public DebugLine(BaseScene? scene, GraphicsResources gfxRes, TimeSpan? disappearsAfter, params Vector3[] points) {
        _scene = scene;
        var shader = gfxRes.ShaderCache.GetShader<LineShader, LineShader.Vertex>();
        var mat = shader.CreateMaterial(Enumerable.Empty<ICBTexture>(), Enumerable.Empty<Sampler>());
        var mesh = new CBMesh<LineShader.Vertex>(gfxRes.GraphicsDevice, points
            .Select(x => new LineShader.Vertex { Position = x })
            .ToArray(), points.Select((_, i) => (uint)i).ToArray());
        var constants = shader.TryCreateInstanceConstants()!;
        constants.SetValue<IWorldMatrixConstantMember, Matrix4x4>(Matrix4x4.Identity);
        _model = new CBModel<LineShader.Vertex>(constants, mat, mesh);
        _model.ConstantProviders.Add(this);
        _countDown = disappearsAfter.HasValue ? (float)disappearsAfter.Value.TotalSeconds : float.PositiveInfinity;
    }

    public DebugLine(GraphicsResources gfxRes, params Vector3[] points) : this(null, gfxRes, null, points) { }

    public void Render(IRenderTarget target, float interp) {
        target.Render(_model, interp);
    }

    protected override void DisposeImpl() {
        _model.Material.Dispose();
        _model.Mesh.Dispose();
    }

    public void Update(float delta) {
        _countDown -= delta;
        if (_countDown <= 0) {
            _scene?.RemoveEntity(this);
        }
    }
}
