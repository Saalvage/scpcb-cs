using System.Numerics;
using Assimp;
using scpcb;
using scpcb.Collision;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.RoomProviders;
using Veldrid;
using static System.Net.Mime.MediaTypeNames;
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

var model2 = new TestAssimpMeshConverter(logoMat).CreateModel(gfx, "Assets/173_2.b3d");
model2.WorldTransform = model2.WorldTransform with { Position = new(0, 0, 5) };

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

var r = new RMeshRoomProvider();
var aaa = r.LoadRoom("Assets/008_opt.rmesh", gfxRes);
var aaaMesh = new Model(aaa.Meshes);
var aaaColl = new CollisionMeshCollection(aaa.CollisionMeshes);

var modelA = new Model(testmesh);
var modelB = new Model(testmesh);
modelB.WorldTransform = modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };

var p1 = new Vector3( 288, 1, - 704);
var p2 = new Vector3(288, 0, - 704);
var p3 = new Vector3(288, 0, -704);

var cross = Vector3.Cross(p3 - p1, p2 - p1);
    
var testetstset = CollideRRR.TriangleCollide(
    new(-10.205289f, 184.80672f, -812.53687f), new(-4.2100883f, 182.80685f, -712.73676f), 150, 10f,
    new Vector3(-200, 0, -704), new (288, 320, -704), new Vector3(288, 0, -704));

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
        var oldPos = controller.Camera.Position;
        controller.HandleMove(Vector2.Normalize(dir), delta);
        var newPos = CollideRRR.TryMove(oldPos * 1000, controller.Camera.Position * 1000, 150f, 10f, aaaColl) * 0.001f;
        if (newPos.X != newPos.X) {
            CollideRRR.TryMove(oldPos * 1000, controller.Camera.Position * 1000, 150f, 10f, aaaColl);
        }
        controller.Camera.Position = newPos;
    }

    Console.WriteLine(aaaColl.Collide(controller.Camera.Position * 1000, 1000 * controller.Camera.Position + Vector3.Transform(new(0, 0, 100), controller.Camera.Rotation), 150f, 10f).Hit);

    modelShader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(
        new Transform(new(0, 0, 0), Quaternion.CreateFromYawPitchRoll(0 / 100, 0, 0), Vector3.One).GetMatrix());
    //mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
    //mesh.Render(commandsList);
    model2.WorldTransform = model2.WorldTransform with { Position = CollisionMeshCollection.Hit };
    model2.Render(commandsList, 0f);
    modelA.Render(commandsList, 0f);
    modelB.Render(commandsList, 0f);
    aaaMesh.Render(commandsList, 0f);
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
    Thread.Sleep(1000 / 60);
}
