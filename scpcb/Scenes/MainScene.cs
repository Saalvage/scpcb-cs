using System.Numerics;
using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Physics;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Scenes;

public class MainScene : Scene3D {
    private readonly GraphicsResources _gfxRes;
    private readonly Player _controller = new();

    private readonly BillboardManager _billboardManager;

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly Matrix4x4 _proj;

    public MainScene(GraphicsResources gfxRes) : base(gfxRes) {
        AddEntity(Physics);

        Camera = _controller.Camera;

        _gfxRes = gfxRes;

        var gfx = gfxRes.GraphicsDevice;
        var window = gfxRes.Window;

        var video = new Video(gfxRes, "Assets/Splash_UTG.mp4");
        AddEntity(video);

        var modelShader = gfxRes.ShaderCache.GetShader<ModelShader, ModelShader.Vertex>();

        // TODO: How do we deal with this? A newly created shader also needs to have the global shader constant providers applied.
        _proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        window.KeyDown += x => _keysDown[x.Key] = true;
        window.KeyUp += x => _keysDown[x.Key] = false;

        var coolTexture = gfxRes.TextureCache.GetTexture("Assets/173texture.jpg");
        var logoMat = modelShader.CreateMaterial(video.Texture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        var otherMat = modelShader.CreateMaterial(coolTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _billboardManager = new(gfxRes);
        var billboard = _billboardManager.Create(video.Texture);
        billboard.Transform = billboard.Transform with { Position = new(2, 0, -0.1f) };
        AddEntity(billboard);

        var room008 = gfxRes.LoadRoom(Physics, _billboardManager, "Assets/Rooms/008/008_opt.rmesh");
        var room4Tunnels = gfxRes.LoadRoom(Physics, _billboardManager, "Assets/Rooms/4tunnels/4tunnels_opt.rmesh");
        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var room = (i == 0 || i == 4 || j == 0 || j == 9 ? room008 : room4Tunnels).Instantiate(new(j * -20.5f, 0, i * -20.5f),
                    Quaternion.CreateFromYawPitchRoll(i % 2 == 0 ? MathF.PI : 0 + j % 2 == 0 ? MathF.PI : 0, 0, 0));
                AddEntity(room);
                AddEntities(room.Entites);
            }
        }

        var sim = Physics.Simulation;

        var (scp173, hull) = new PluginAssimpMeshConverter<ModelShader.Vertex>(ModelShader.Vertex.ConvertVertex, _ => logoMat)
            .LoadMeshes(gfx, Physics, "Assets/173_2.b3d");

        window.KeyDown += x => {
            if (x.Key == Key.Space) {
                var bodyHandle = sim.Bodies.Add(BodyDescription.CreateConvexDynamic(
                        new(_controller.Camera.Position, _controller.Camera.Rotation), new(10 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation)),
                    1, sim.Shapes, hull));
                var bodyRef = sim.Bodies.GetBodyReference(bodyHandle);
                AddEntity(new PhysicsModelCollection(Physics, bodyRef, new[] { new CBModel<ModelShader.Vertex>(
                    modelShader.TryCreateInstanceConstants(), Random.Shared.NextSingle() > 0.5 ? otherMat : logoMat, scp173[0].Mesh) }));
            }
        };
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
}
