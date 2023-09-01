using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Map;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Scenes;

public class MainScene : Scene3D {
    private readonly Game _game;
    private readonly GraphicsResources _gfxRes;
    
    private readonly Player _controller = new();

    private readonly BillboardManager _billboardManager;

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly Matrix4x4 _proj;
    private readonly ConvexHull _hull;
    private readonly ICBMaterial<ModelShader.Vertex> _renderMat;
    private readonly ICBMaterial<ModelShader.Vertex> _otherMat;
    private readonly ICBMaterial<ModelShader.Vertex> _logoMat;
    private readonly ICBModel<ModelShader.Vertex> _scp173;

    private readonly IRoomData _room008;
    private readonly IRoomData _room4Tunnels;

    private readonly RenderTexture _renderTexture;

    public MainScene(Game game) : base(game.GraphicsResources) {
        _game = game;
        _gfxRes = game.GraphicsResources;

        AddEntity(Physics);

        AddEntity(new DebugLine(this, _gfxRes, TimeSpan.FromSeconds(5), new(0, 0, 0), new(1, 1, 1), new(5, 1, 1)));

        Camera = _controller.Camera;

        _renderTexture = new(_gfxRes, 100, 100, true);

        var gfx = _gfxRes.GraphicsDevice;
        var window = _gfxRes.Window;

        var video = new Video(_gfxRes, "Assets/Splash_UTG.mp4");
        video.Loop = true;
        AddEntity(video);

        var modelShader = _gfxRes.ShaderCache.GetShader<ModelShader, ModelShader.Vertex>();

        // TODO: How do we deal with this? A newly created shader also needs to have the global shader constant providers applied.
        _proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        var coolTexture = _gfxRes.TextureCache.GetTexture("Assets/173texture.jpg");
        _logoMat = modelShader.CreateMaterial(video.Texture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _otherMat = modelShader.CreateMaterial(coolTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _renderMat = modelShader.CreateMaterial(_renderTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _billboardManager = new(_gfxRes);
        var billboard = _billboardManager.Create(_renderTexture);
        billboard.Transform = billboard.Transform with { Position = new(2, 2, -0.1f) };
        AddEntity(billboard);

        _room008 = _gfxRes.LoadRoom(Physics, _billboardManager, "Assets/Rooms/008/008_opt.rmesh");
        _room4Tunnels = _gfxRes.LoadRoom(Physics, _billboardManager, "Assets/Rooms/4tunnels/4tunnels_opt.rmesh");
        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var room = (i == 0 || i == 4 || j == 0 || j == 9 ? _room008 : _room4Tunnels).Instantiate(new(j * -20.5f, 0, i * -20.5f),
                    Quaternion.CreateFromYawPitchRoll(i % 2 == 0 ? MathF.PI : 0 + j % 2 == 0 ? MathF.PI : 0, 0, 0));
                AddEntity(room);
            }
        }

        var (scp173, hull) = new PluginAssimpMeshConverter<ModelShader.Vertex>(ModelShader.Vertex.ConvertVertex,
                _ => _logoMat)
            .LoadMeshes(gfx, Physics, "Assets/173_2.b3d");

        _scp173 = scp173[0];
        _hull = hull;

        window.KeyDown += HandleKeyDown;
        window.KeyUp += HandleKeyUp;
    }

    public override void Update(float delta) {
        if (_gfxRes.Window.MouseDelta != Vector2.Zero) {
            _controller.HandleMouse(_gfxRes.Window.MouseDelta * 0.01f);
        }

        var dir = Vector2.Zero;
        if (KeyDown(Key.W)) dir += Vector2.UnitY;
        if (KeyDown(Key.S)) dir -= Vector2.UnitY;
        if (KeyDown(Key.A)) dir += Vector2.UnitX;
        if (KeyDown(Key.D)) dir -= Vector2.UnitX;

        if (dir != Vector2.Zero) {
            _controller.HandleMove(Vector2.Normalize(dir), delta);
        }

        foreach (var sh in _gfxRes.ShaderCache.ActiveShaders) {
            sh.Constants?.SetValue<IProjectionMatrixConstantMember, Matrix4x4>(_proj);
        }

        base.Update(delta);
    }

    public override void Render(IRenderTarget target, float interp) {
        // TODO: This should offer great opportunities for optimization & parallelization!
        _renderTexture.Start();
        base.Render(_renderTexture, interp);
        _renderTexture.End();
        base.Render(target, interp);
    }

    public override void OnLeave() {
        _gfxRes.Window.KeyDown -= HandleKeyDown;
        _gfxRes.Window.KeyUp -= HandleKeyUp;
    }

    private void HandleKeyDown(KeyEvent e) {
        _keysDown[e.Key] = true;

        if (e.Key == Key.Space) {
            var sim = Physics.Simulation;
            var bodyHandle = sim.Bodies.Add(BodyDescription.CreateConvexDynamic(
                new(_controller.Camera.Position, _controller.Camera.Rotation), new(10 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation)),
                1, sim.Shapes, _hull));
            var bodyRef = sim.Bodies.GetBodyReference(bodyHandle);
            AddEntity(new PhysicsModelCollection(Physics, bodyRef, new[] { new CBModel<ModelShader.Vertex>(
                _gfxRes.ShaderCache.GetShader<ModelShader, ModelShader.Vertex>().TryCreateInstanceConstants(), Random.Shared.Next(3) switch {
                    0 => _renderMat,
                    1 => _otherMat,
                    2 => _logoMat,
                }, _scp173.Mesh)}));
        } else if (e.Key == Key.Escape) {
            _game.Scene = new VideoScene(_game, "Assets/Splash_UTG.mp4");
        } else if (e.Key == Key.AltLeft) {
            var from = _controller.Camera.Position;
            var to = from + Vector3.Transform(Vector3.UnitZ, _controller.Camera.Rotation) * 5f;
            var line = new DebugLine(_gfxRes, from, to);
            line.Color = Physics.RayCastVisible(from, to) ? new(1, 0, 0) : new(0, 1, 0);
            AddEntity(line);
        }
    }

    private void HandleKeyUp(KeyEvent e) {
        _keysDown[e.Key] = false;
    }

    protected override void DisposeImpl() {
        _room008.Dispose();
        _room4Tunnels.Dispose();
        _renderMat.Dispose();
        _renderTexture.Dispose();
        base.DisposeImpl();
    }
}
