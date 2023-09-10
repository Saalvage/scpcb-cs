using System.Numerics;
using BepuPhysics.Collidables;
using scpcb.Graphics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Vertices;
using scpcb.Graphics.Textures;
using scpcb.Map;
using scpcb.Physics;
using scpcb.Physics.Primitives;
using scpcb.Serialization;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Scenes;

public class MainScene : Scene3D {
    private readonly Game _game;
    private readonly GraphicsResources _gfxRes;
    
    private readonly Player _controller = new();

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly Matrix4x4 _proj;
    private readonly ICBShape<ConvexHull> _hull;
    private readonly ICBMaterial<VPositionTexture> _renderMat;
    private readonly ICBMaterial<VPositionTexture> _otherMat;
    private readonly ICBMaterial<VPositionTexture> _logoMat;
    private readonly ICBModel<VPositionTexture> _scp173;

    private readonly IRoomData _room008;
    private readonly IRoomData _room4Tunnels;

    private readonly RenderTexture _renderTexture;
    private readonly ModelCache.CacheEntry _cacheEntry;

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

        var modelShader = _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>();

        // TODO: How do we deal with this? A newly created shader also needs to have the global shader constant providers applied.
        _proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        var coolTexture = _gfxRes.TextureCache.GetTexture("Assets/173texture.jpg");
        _logoMat = _gfxRes.MaterialCache.GetMaterial(modelShader, video.Texture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _otherMat = _gfxRes.MaterialCache.GetMaterial(modelShader, coolTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _renderMat = _gfxRes.MaterialCache.GetMaterial(modelShader, _renderTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        var billboard = Billboard.Create(_gfxRes, _renderTexture);
        billboard.Transform = billboard.Transform with { Position = new(2, 2, -0.1f) };
        AddEntity(billboard);

        _room008 = _gfxRes.LoadRoom(Physics, "Assets/Rooms/008/008_opt.rmesh");
        _room4Tunnels = _gfxRes.LoadRoom(Physics, "Assets/Rooms/4tunnels/4tunnels_opt.rmesh");
        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var room = (i == 0 || i == 4 || j == 0 || j == 9 ? _room008 : _room4Tunnels).Instantiate(new(j * -20.5f, 0, i * -20.5f),
                    Quaternion.CreateFromYawPitchRoll(i % 2 == 0 ? MathF.PI : 0 + j % 2 == 0 ? MathF.PI : 0, 0, 0));
                AddEntity(room);
            }
        }

        _cacheEntry = Physics.ModelCache.GetModel("Assets/173_2.b3d"); ;
        _scp173 = _cacheEntry.Models.Instantiate().OfType<ICBModel<VPositionTexture>>().First();
        _hull = _cacheEntry.Collision;

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
        // On second consideration: Most render targets will differ in e.g. view position
        // meaning potential for optimization might not actually be there. :(
        _renderTexture.Start();
        var a = Task.Run(() => {
            base.Render(_renderTexture, interp);
            _renderTexture.End();
        });
        var b = Task.Run(() => {
            base.Render(target, interp);
        });
        Task.WaitAll(a, b);
    }

    public override void OnLeave() {
        // TODO: This sucks! Might as well eliminate the entire method.
        _gfxRes.Window.KeyDown -= HandleKeyDown;
        _gfxRes.Window.KeyUp -= HandleKeyUp;
    }

    private static string? _serialized;

    private void HandleKeyDown(KeyEvent e) {
        _keysDown[e.Key] = true;

        switch (e.Key) {
            case Key.Space: {
                var sim = Physics.Simulation;
                var body = _hull.CreateDynamic(new(_controller.Camera.Position, _controller.Camera.Rotation), 1);
                body.Velocity = new(10 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation));
                AddEntity(new PhysicsModelCollection(Physics, body, new[] { new CBModel<VPositionTexture>(
                    _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>().TryCreateInstanceConstants(), Random.Shared.Next(3) switch {
                        0 => _renderMat,
                        1 => _otherMat,
                        2 => _logoMat,
                    }, _scp173.Mesh)}));
                break;
            }
            case Key.Escape:
                _game.Scene = new VideoScene(_game, "Assets/Splash_UTG.mp4");
                break;
            case Key.AltLeft: {
                var from = _controller.Camera.Position;
                var to = from + Vector3.Transform(Vector3.UnitZ, _controller.Camera.Rotation) * 5f;
                var line = new DebugLine(_gfxRes, from, to);
                line.Color = Physics.RayCastVisible(from, to) ? new(1, 0, 0) : new(0, 1, 0);
                AddEntity(line);
                break;
            }
            case Key.F5: {
                _serialized = SerializationHelper.SerializeTest(GetEntitiesOfType<ISerializableEntity>());
                foreach (var i in GetEntitiesOfType<ISerializableEntity>()) {
                    RemoveEntity(i);
                }
                break;
            }
            case Key.BackSpace: {
                if (_serialized != null) {
                    AddEntities(SerializationHelper.DeserializeTest(_serialized, _gfxRes, this));
                }
                break;
            }
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
