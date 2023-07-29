using System.Numerics;
using BepuPhysics;
using scpcb;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Physics;
using scpcb.RoomProviders;
using Veldrid;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

const int WIDTH = 1280;
const int HEIGHT = 720;

using var gfxRes = new GraphicsResources(WIDTH, HEIGHT);
var gfx = gfxRes.GraphicsDevice;

var factory = gfx.ResourceFactory;

//using var shader2 = new UIShader(gfx);
//shader2.VertexConstants.Projection = Matrix4x4.CreateOrthographic(WIDTH, HEIGHT, 0.1f, 100f);

using var coolTexture = new CBTexture(gfx, "Assets/scp.jpg");
//using var mesh = new UIMesh(gfx, shader2, coolTexture);

using var commandsList = factory.CreateCommandList();

var countingTo = DateTimeOffset.Now;
var fps = 0;

var modelShader = gfxRes.ShaderCache.GetShader<ModelShaderGenerated>();

var rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShaderGenerated>();
var logoMat = modelShader.CreateMaterial(coolTexture);
using var testmesh = new CBMesh<ModelShader.Vertex>(gfx, logoMat,
    new ModelShader.Vertex[] {
        new(new(-1f, 1f, 0), new(1, 0)),
        new(new(1f, 1f, 0), new(0, 0)),
        new(new(-1f, -1f, 0), new(1, 1)),
        new(new(1f, -1f, 0), new(0, 1)),
    },
    new uint[] { 0, 1, 2, 3, 2, 1 });

var scp173 = new TestAssimpMeshConverter(logoMat).LoadMeshes(gfx, "Assets/173_2.b3d");

modelShader.VertexConstants.ViewMatrix = rMeshShader.VertexConstants.ViewMatrix
    = Matrix4x4.CreateLookAt(new(0, 0, -5), Vector3.UnitZ, Vector3.UnitY);
modelShader.VertexConstants.ProjectionMatrix = rMeshShader.VertexConstants.ProjectionMatrix
    = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)WIDTH / HEIGHT, 0.1f, 10000f);

Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

var controller = new CharacterController();

var KeysDown = new Dictionary<Key, bool>();

var window = gfxRes.Window;
window.KeyDown += x => KeysDown[x.Key] = true;
window.KeyUp += x => KeysDown[x.Key] = false;
bool KeyDown(Key x) => KeysDown.TryGetValue(x, out var y) && y;

var physics = new PhysicsResources();

var r = new RMeshRoomProvider();
var (aaa, aaaShape) = r.Test("Assets/008_opt.rmesh", gfxRes, physics);

var modelA = new Model(testmesh);
var modelB = new Model(testmesh);
modelB.WorldTransform = modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };

var tIndex = physics.Simulation.Shapes.Add(aaaShape);
physics.Simulation.Statics.Add(new(new(), tIndex));

var physicsModels = physics.Bodies.Select(x => new PhysicsModel(x, scp173)).ToList();

window.KeyDown += x => {
    if (x.Key == Key.Space) {
        var refff = physics.Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                controller.Camera.Position, new(100 * Vector3.Transform(new(0, 0, 1), controller.Camera.Rotation)),
            PhysicsResources.BoxInertia, physics.BoxIndex, 0.01f));
        var reff = physics.Simulation.Bodies.GetBodyReference(refff);
        physics.Bodies.Add(reff);
        physicsModels.Add(new(reff, scp173));
    }
};

var now = DateTime.UtcNow;
while (window.Exists) {
    window.PumpEvents();
    if (window.MouseDelta != Vector2.Zero) {
        controller.HandleMouse(window.MouseDelta * 0.01f);
    }

    modelShader.VertexConstants.ViewMatrix = rMeshShader.VertexConstants.ViewMatrix = controller.Camera.ViewMatrix;
    commandsList.Begin();
    commandsList.SetFramebuffer(gfx.SwapchainFramebuffer);
    commandsList.ClearColorTarget(0, RgbaFloat.Grey);
    commandsList.ClearDepthStencil(1);

    var oldNow = now;
    now = DateTime.UtcNow;
    var delta = (float)(now - oldNow).TotalSeconds / 16;

    var dir = Vector2.Zero;
    if (KeyDown(Key.W)) dir += Vector2.UnitY;
    if (KeyDown(Key.S)) dir -= Vector2.UnitY;
    if (KeyDown(Key.A)) dir += Vector2.UnitX;
    if (KeyDown(Key.D)) dir -= Vector2.UnitX;

    if (dir != Vector2.Zero) {
        controller.HandleMove(Vector2.Normalize(dir), delta);
    }
    
    modelShader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(
        new Transform(new(0, 0, 0), Quaternion.CreateFromYawPitchRoll(0 / 100, 0, 0), Vector3.One).GetMatrix());
    //mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
    //mesh.Render(commandsList);
    model2.Render(commandsList, 0f);
    modelA.Render(commandsList, 0f);
    modelB.Render(commandsList, 0f);
    foreach (var meshh in aaa) {
        rMeshShader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(new Transform().GetMatrix());
        meshh.Render(commandsList);
    }
    physics.Update(delta);
    foreach (var reff in physicsModels) {
        reff.Render(commandsList, 0f);
    }
    commandsList.End();
    gfx.SubmitCommands(commandsList);
    gfx.SwapBuffers();
    fps++;
    var noww = DateTimeOffset.Now;
    if (noww > countingTo) {
        Console.WriteLine(fps);
        fps = 0;
        countingTo = noww.AddSeconds(1);
    }
}
