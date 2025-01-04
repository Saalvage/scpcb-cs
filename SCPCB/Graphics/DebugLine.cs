using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Textures;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Graphics;

public class DebugLine : Disposable, IRenderable, IUpdatable, IConstantProvider<IColorConstantMember, Vector3> {
    private readonly IScene? _scene;

    private readonly ICBModel _model;

    public Vector3 Color { get; set; } = new(1, 0, 0);
    Vector3 IConstantProvider<IColorConstantMember, Vector3>.GetValue(float interp) => Color;

    private float _countDown;

    private DebugLine(IScene? scene, GraphicsResources gfxRes, TimeSpan? disappearsAfter, params Vector3[] points) {
        _scene = scene;
        var shader = gfxRes.ShaderCache.GetShader<LineShader, VPosition>();
        var mat = gfxRes.MaterialCache.GetMaterial(shader, [], []);
        var mesh = new CBMesh<VPosition>(gfxRes.GraphicsDevice, points
            .Select(x => new VPosition { Position = x })
            .ToArray(), points.Select((_, i) => (uint)i).ToArray());
        var constants = shader.TryCreateInstanceConstants()!;
        constants.SetValue<IWorldMatrixConstantMember, Matrix4x4>(Matrix4x4.Identity);
        _model = new CBModel<VPosition>(constants, mat, mesh);
        _model.ConstantProviders.Add(this);
        _countDown = disappearsAfter.HasValue ? (float)disappearsAfter.Value.TotalSeconds : float.PositiveInfinity;
    }

    public DebugLine(IScene scene, TimeSpan disappearsAfter, params Vector3[] points) : this(scene, scene.Graphics, disappearsAfter, points) { }

    public DebugLine(GraphicsResources gfxRes, params Vector3[] points) : this(null, gfxRes, null, points) { }

    public void Render(IRenderTarget target, float interp) {
        _model.Render(target, interp);
    }

    protected override void DisposeImpl() {
        _model.Mesh.Dispose();
    }

    public void Update(float delta) {
        _countDown -= delta;
        if (_countDown <= 0) {
            _scene?.RemoveEntity(this);
        }
    }
}
