using System.Diagnostics;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Textures;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics;

public class DreamFilter : Disposable, ITickable {
    private readonly Action<IRenderTarget, float> _renderBaseScene;

    public ICBTexture SceneTexture => _sceneTexture;

    private readonly RenderTexture _sceneTexture;
    private readonly RenderTexture _dreamTexture;

    // Used to render a blurred version of the scene texture onto the dream texture.
    private readonly IMeshInstance _blurredSceneRenderer;
    // Used to render the dream overlay onto the scene texture.
    private readonly IMeshInstance _dreamRenderer;
    // Used to render the scene texture onto the main framebuffer.
    private readonly IMeshInstance _sceneRenderer;

    private int _currentTick;

    public int TicksPerCycle {
        get;
        set {
            Debug.Assert(value != 0);
            field = value;
        }
    } = 1;

    public float BlurFactor {
        get;
        set {
            Debug.Assert(value < 1f, "Blur factor must be below 1. Use 0 to disable the effect.");
            field = value;
            _blurredSceneRenderer.Constants!.SetValue<IBlurStrengthConstantMember, float>(value);
        }
    }

    /// <summary>
    /// Offset of the dream overlay in pixels.
    /// <remarks>Creates the streak effect.</remarks>
    /// </summary>
    public Vector2 Offset {
        get;
        set {
            field = value;
            _blurredSceneRenderer.Constants!.SetValue<IPositionConstantMember, Vector3>(new(value, 0));
        }
    }

    public DreamFilter(GraphicsResources gfxRes, Action<IRenderTarget, float> renderBaseScene) {
        var gfx = gfxRes.GraphicsDevice;

        _renderBaseScene = renderBaseScene;

        _sceneTexture = new(gfxRes, (uint)gfxRes.Window.Width, (uint)gfxRes.Window.Height, true);
        _dreamTexture = new(gfxRes, (uint)gfxRes.Window.Width, (uint)gfxRes.Window.Height) {
            ClearColor = null,
            ClearDepth = false,
        };

        var mesh = new CBMesh<VPositionTexture>(gfx, [
            new(new(0, 0, 0), new(0, 0)),
            new(new(gfxRes.Window.Width, 0, 0), new(1, 0)),
            new(new(0, gfxRes.Window.Height, 0), new(0, 1)),
            new(new(gfxRes.Window.Width, gfxRes.Window.Height, 0), new(1, 1)),
        ], [2, 1, 0, 1, 2, 3]);

        var shader = gfxRes.ShaderCache.GetShader<DreamShader, VPositionTexture>();
        var overrideBlendShader = gfxRes.ShaderCache.GetShader<DreamShader, VPositionTexture>(x => x with { BlendState = BlendStateDescription.SingleOverrideBlend });

        // Constants used to create the actual blur effect.
        var mainConstants = shader.TryCreateInstanceConstants()!;
        // Constants used to simply render one texture onto another.
        var utilityConstants = shader.TryCreateInstanceConstants()!;
        utilityConstants.SetValue<IBlurStrengthConstantMember, float>(1f);

        _blurredSceneRenderer = new MeshInstance<VPositionTexture>(mainConstants, gfxRes.MaterialCache.GetMaterial(shader,
            [_sceneTexture], [gfx.PointSampler]), mesh);

        _dreamRenderer = new MeshInstance<VPositionTexture>(utilityConstants, gfxRes.MaterialCache.GetMaterial(shader,
            [_dreamTexture], [gfx.PointSampler]), mesh);

        _sceneRenderer = new MeshInstance<VPositionTexture>(utilityConstants, gfxRes.MaterialCache.GetMaterial(overrideBlendShader,
            [_sceneTexture], [gfx.PointSampler]), mesh);
    }

    public void Tick() {
        _currentTick++;
        if (_currentTick >= TicksPerCycle) {
            _currentTick = 0;

            _dreamTexture.Start();
            _blurredSceneRenderer.Render(_dreamTexture, 0);
            _dreamTexture.End();
        }
    }

    public void RenderScene(IRenderTarget target, float interp) {
        _sceneTexture.Start();
        _renderBaseScene(_sceneTexture, interp);
        _dreamRenderer.Render(_sceneTexture, interp);
        _sceneTexture.End();

        _sceneRenderer.Render(target, interp);
    }

    protected override void DisposeImpl() {
        _sceneTexture.Dispose();
        _dreamTexture.Dispose();
        _blurredSceneRenderer.Constants!.Dispose();
        _sceneRenderer.Constants!.Dispose();
        _sceneRenderer.Mesh.Dispose();
    }
}
