using System.Numerics;
using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Map.Entities;
using scpcb.Physics;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Scenes;

public class MainScene : Scene3D {
    public PhysicsResources Physics { get; } = new();

    private readonly GraphicsResources _gfxRes;
    private readonly Player _controller = new();

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    public MainScene(GraphicsResources gfxRes) : base(gfxRes) {
        AddEntity(Physics);

        Camera = _controller.Camera;

        _gfxRes = gfxRes;

        var gfx = gfxRes.GraphicsDevice;
        var window = gfxRes.Window;

        var modelShader = gfxRes.ShaderCache.GetShader<ModelShader, ModelShader.Vertex>();
        var rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShader, RMeshShader.Vertex>();

        // TODO: How do we deal with this? A newly created shader also needs to have the global shader constant providers applied.
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f);
        modelShader.Constants.SetValue<IProjectionMatrixConstantMember, Matrix4x4>(proj);
        rMeshShader.Constants.SetValue<IProjectionMatrixConstantMember, Matrix4x4>(proj);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        window.KeyDown += x => _keysDown[x.Key] = true;
        window.KeyUp += x => _keysDown[x.Key] = false;

        var room008 = gfxRes.LoadRoom(Physics, "Assets/Rooms/008/008_opt.rmesh");
        var room4Tunnels = gfxRes.LoadRoom(Physics, "Assets/Rooms/4tunnels/4tunnels_opt.rmesh");
        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var room = (i == 0 || i == 4 || j == 0 || j == 9 ? room008 : room4Tunnels).Instantiate(new(j * -20.5f, 0, i * -20.5f),
                    Quaternion.CreateFromYawPitchRoll(i % 2 == 0 ? MathF.PI : 0 + j % 2 == 0 ? MathF.PI : 0, 0, 0));
                AddEntity(room);
                AddEntities(room.Entites.OfType<Model>().Select(x => x.Models));
            }
        }

        var coolTexture = gfxRes.TextureCache.GetTexture("Assets/scp.jpg");
        var logoMat = modelShader.CreateMaterial(coolTexture.AsEnumerableElement());
        var testMesh = new CBModel<ModelShader.Vertex>(modelShader.TryCreateInstanceConstants(), logoMat,
            new CBMesh<ModelShader.Vertex>(gfx, new ModelShader.Vertex[] {
                new(new(-1f, 1f, 0), new(1, 0)),
                new(new(1f, 1f, 0), new(0, 0)),
                new(new(-1f, -1f, 0), new(1, 1)),
                new(new(1f, -1f, 0), new(0, 1)),
            },
            new uint[] { 0, 1, 2, 3, 2, 1 }));

        ModelCollection modelB = new(new[] { testMesh });
        modelB.WorldTransform = modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };
        AddEntity(modelB);

        var sim = Physics.Simulation;

        var (scp173, hull) = new AutomaticAssimpMeshConverter<ModelShader, ModelShader.Vertex, ICBMaterial<ModelShader.Vertex>>(logoMat)
            .LoadMeshes(gfx, Physics, "Assets/173_2.b3d");

        window.KeyDown += x => {
            if (x.Key == Key.Space) {
                var bodyHandle = sim.Bodies.Add(BodyDescription.CreateConvexDynamic(
                        new(_controller.Camera.Position, _controller.Camera.Rotation), new(10 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation)),
                    1, sim.Shapes, hull));
                var bodyRef = sim.Bodies.GetBodyReference(bodyHandle);
                AddEntity(new PhysicsModelCollection(Physics, bodyRef, new[] { new CBModel<ModelShader.Vertex>(
                    modelShader.TryCreateInstanceConstants(), scp173[0].Material, scp173[0].Mesh) }));
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

        base.Update(delta);
    }
}
