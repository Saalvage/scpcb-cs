using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.RoomProviders;
using System.Numerics;
using BepuPhysics.Collidables;
using Veldrid;

namespace scpcb;

public class MainScene : Disposable , IScene {
    public PhysicsResources Physics { get; } = new();

    private readonly GraphicsResources _gfxRes;
    private readonly CharacterController _controller = new();

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly List<PhysicsModel> _physicsModels;
    private readonly IReadOnlyList<ICBMesh> _aaa; // TODO: Why list?

    private readonly RMeshShaderGenerated _rMeshShader;
    private readonly ModelShaderGenerated _modelShader;

    private readonly Model _modelA;
    private readonly Model _modelB;

    public MainScene(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;

        var gfx = gfxRes.GraphicsDevice;

        //using var shader2 = new UIShader(gfx);
        //shader2.VertexConstants.Projection = Matrix4x4.CreateOrthographic(WIDTH, HEIGHT, 0.1f, 100f);

        var coolTexture = new CBTexture(gfx, "Assets/scp.jpg");
        //using var mesh = new UIMesh(gfx, shader2, coolTexture);

        _modelShader = gfxRes.ShaderCache.GetShader<ModelShaderGenerated>();

        _rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShaderGenerated>();
        var logoMat = _modelShader.CreateMaterial(coolTexture);
        var testMesh = new CBMesh<ModelShader.Vertex>(gfx, logoMat,
            new ModelShader.Vertex[] {
                new(new(-1f, 1f, 0), new(1, 0)),
                new(new(1f, 1f, 0), new(0, 0)),
                new(new(-1f, -1f, 0), new(1, 1)),
                new(new(1f, -1f, 0), new(0, 1)),
            },
            new uint[] { 0, 1, 2, 3, 2, 1 });

        var scp173 = new TestAssimpMeshConverter(logoMat).LoadMeshes(gfx, "Assets/173_2.b3d");

        var window = gfxRes.Window;

        _modelShader.VertexConstants.ViewMatrix = _rMeshShader.VertexConstants.ViewMatrix
            = Matrix4x4.CreateLookAt(new(0, 0, -5), Vector3.UnitZ, Vector3.UnitY);
        _modelShader.VertexConstants.ProjectionMatrix = _rMeshShader.VertexConstants.ProjectionMatrix
            = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 10000f);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        window.KeyDown += x => _keysDown[x.Key] = true;
        window.KeyUp += x => _keysDown[x.Key] = false;

        var r = new RMeshRoomProvider();
        Mesh aaaShape;
        (_aaa, aaaShape) = r.Test("Assets/008_opt.rmesh", gfxRes, Physics);

        _modelA = new(testMesh);
        _modelB = new(testMesh);
        _modelB.WorldTransform = _modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };

        var sim = Physics.Simulation;

        var tIndex = sim.Shapes.Add(aaaShape);
        sim.Statics.Add(new(new(), tIndex));

        _physicsModels = Physics.Bodies.Select(x => new PhysicsModel(x, scp173)).ToList();

        window.KeyDown += x => {
            if (x.Key == Key.Space) {
                var refff = sim.Bodies.Add(BodyDescription.CreateDynamic(
                        _controller.Camera.Position, new(100 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation)),
                    PhysicsResources.BoxInertia, Physics.BoxIndex, 0.01f));
                var reff = sim.Bodies.GetBodyReference(refff);
                Physics.Bodies.Add(reff);
                _physicsModels.Add(new(reff, scp173));
            }
        };
    }

    public void Update(double delta) {
        if (_gfxRes.Window.MouseDelta != Vector2.Zero) {
            _controller.HandleMouse(_gfxRes.Window.MouseDelta * 0.01f);
        }

        var dir = Vector2.Zero;
        if (KeyDown(Key.W)) dir += Vector2.UnitY;
        if (KeyDown(Key.S)) dir -= Vector2.UnitY;
        if (KeyDown(Key.A)) dir += Vector2.UnitX;
        if (KeyDown(Key.D)) dir -= Vector2.UnitX;

        if (dir != Vector2.Zero) {
            _controller.HandleMove(Vector2.Normalize(dir), (float)delta);
        }
    }

    public void Tick() {
        Physics.Update(1f / Game.TICK_RATE);
    }

    public void Render(CommandList commandsList, float interp) {
        _modelShader.VertexConstants.ViewMatrix = _rMeshShader.VertexConstants.ViewMatrix = _controller.Camera.ViewMatrix;

        _modelShader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(
            new Transform(new(0, 0, 0), Quaternion.CreateFromYawPitchRoll(0 / 100, 0, 0), Vector3.One).GetMatrix());
        //mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
        //mesh.Render(commandsList);
        _modelA.Render(commandsList, interp);
        _modelB.Render(commandsList, interp);
        foreach (var meshh in _aaa) {
            _rMeshShader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(new Transform().GetMatrix());
            meshh.Render(commandsList);
        }
        
        foreach (var reff in _physicsModels) {
            reff.Render(commandsList, interp);
        }
    }

    protected override void DisposeImpl() {
        Physics.Dispose();
    }
}
