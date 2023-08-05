using BepuPhysics;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.RoomProviders;
using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using Veldrid;
using scpcb.Graphics.ModelCollections;
using scpcb.Utility;

namespace scpcb;

public class MainScene : Disposable , IScene {
    public PhysicsResources Physics { get; } = new();

    private readonly GraphicsResources _gfxRes;
    private readonly CharacterController _controller = new();

    private readonly Dictionary<Key, bool> _keysDown = new();
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly List<PhysicsModelCollection> _physicsModels;

    private readonly RMeshShaderGenerated _rMeshShader;
    private readonly ModelShaderGenerated _modelShader;

    private readonly ModelCollection _modelA;
    private readonly ModelCollection _modelB;

    private List<I3DModel> _renderables = new();

    public MainScene(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;

        var gfx = gfxRes.GraphicsDevice;

        _target = gfxRes.MainTarget;

        var coolTexture = new CBTexture(gfxRes, "Assets/scp.jpg");

        _modelShader = gfxRes.ShaderCache.GetShader<ModelShaderGenerated>();

        _rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShaderGenerated>();
        var logoMat = _modelShader.CreateMaterial(coolTexture.AsEnumerableElement());
        var testMesh = new CBModel<ModelShader.Vertex>(_modelShader.TryCreateInstanceConstants(), logoMat,
            new CBMesh<ModelShader.Vertex>(gfx, new ModelShader.Vertex[] {
                new(new(-1f, 1f, 0), new(1, 0)),
                new(new(1f, 1f, 0), new(0, 0)),
                new(new(-1f, -1f, 0), new(1, 1)),
                new(new(1f, -1f, 0), new(0, 1)),
            },
            new uint[] { 0, 1, 2, 3, 2, 1 }));

        var (scp173, hull) = new TestAssimpMeshConverter(logoMat).LoadMeshes(gfx, Physics, "Assets/173_2.b3d");
        var hullIndex = Physics.Simulation.Shapes.Add(hull);

        var window = gfxRes.Window;

        _modelShader.Constants.Vertex.ProjectionMatrix = _rMeshShader.Constants.Vertex.ProjectionMatrix
            = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 10000f);

        Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

        window.KeyDown += x => _keysDown[x.Key] = true;
        window.KeyUp += x => _keysDown[x.Key] = false;

        var (_aaa, aaaShape) = gfxRes.LoadRoom(Physics, "Assets/Rooms/008/008_opt.rmesh");
        _modelA = new(_aaa);
        _renderables.AddRange(_modelA.Models);
        
        _modelB = new(testMesh);
        _modelB.WorldTransform = _modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };

        var sim = Physics.Simulation;

        var tIndex = sim.Shapes.Add(aaaShape);
        sim.Statics.Add(new(new(), tIndex));

        _physicsModels = new();

        window.KeyDown += x => {
            if (x.Key == Key.Space) {
                var refff = sim.Bodies.Add(BodyDescription.CreateConvexDynamic(
                        new(_controller.Camera.Position, _controller.Camera.Rotation), new(10 * Vector3.Transform(new(0, 0, 1), _controller.Camera.Rotation)),
                    1, sim.Shapes, hull));
                var reff = sim.Bodies.GetBodyReference(refff);
                _physicsModels.Add(new(Physics, reff, new CBModel<ModelShader.Vertex>(
                    _modelShader.TryCreateInstanceConstants(), scp173[0].Material, scp173[0].Mesh)));
                _renderables.AddRange(_physicsModels.Last().Models);
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

    private readonly RenderTarget _target;

    public void Render(RenderTarget target, float interp) {
        _modelShader.Constants.Vertex.ViewMatrix = _rMeshShader.Constants.Vertex.ViewMatrix = _controller.Camera.ViewMatrix;

        //mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
        //mesh.UpdateConstants(commandsList);
        //_modelA.UpdateConstants(commandsList, interp);
        //_modelB.UpdateConstants(commandsList, interp);

        foreach (var reff in _physicsModels) {
            reff.UpdateConstants(_target, interp);
        }

        // TODO: Optimize this (hot path)
        var groupings = _renderables.GroupBy(x => x.Model.IsOpaque).ToArray();
        var opaque = groupings.SingleOrDefault(x => x.Key);
        var transparent = groupings.SingleOrDefault(x => !x.Key);

        _modelA.UpdateConstants(target, interp);

        foreach (var renderable in opaque ?? Enumerable.Empty<I3DModel>()) {
            _target.Render(renderable.Model);
        }

        foreach (var renderable in transparent?
                     .OrderByDescending(x => Vector3.DistanceSquared(_controller.Camera.Position, x.Position))
                                   ?? Enumerable.Empty<I3DModel>()) {
            _target.Render(renderable.Model);
        }
    }

    protected override void DisposeImpl() {
        Physics.Dispose();
    }
}
