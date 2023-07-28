using System.Numerics;
using Assimp;
using scpcb;
using scpcb.Graphics;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
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

//var rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShader>();
using var testmesh = new CBMesh<ModelShader.Vertex>(gfx, modelShader.CreateMaterial(coolTexture),
    new ModelShader.Vertex[] {
        new(new(-1f, 1f, 0), new(1, 0)),
        new(new(1f, 1f, 0), new(0, 0)),
        new(new(-1f, -1f, 0), new(1, 1)),
        new(new(1f, -1f, 0), new(0, 1)),
    },
    new uint[] { 0, 1, 2, 3, 2, 1 });

using var assimp = new AssimpContext();
var scene = assimp.ImportFile("Assets/173_2.b3d");
var scp = new TestAssimpMeshConverter();
var mesh2 = scp.ConvertMesh(gfx, scene.Meshes[0], modelShader.CreateMaterial(coolTexture));

modelShader.VertexConstants.ViewMatrix /*= rMeshShader.VertexConstants.View*/
    = Matrix4x4.CreateLookAt(new(0, 0, -5), Vector3.UnitZ, Vector3.UnitY);
modelShader.VertexConstants.ProjectionMatrix /*= rMeshShader.VertexConstants.Projection*/
    = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)WIDTH / HEIGHT, 0.1f, 10000f);

Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

var controller = new CharacterController();

var KeysDown = new Dictionary<Key, bool>();

var window = gfxRes.Window;
window.KeyDown += x => KeysDown[x.Key] = true;
window.KeyUp += x => KeysDown[x.Key] = false;
bool KeyDown(Key x) => KeysDown.TryGetValue(x, out var y) && y;

var r = new RMeshRoomProvider();
//var aaa = r.Test("Assets/008_opt.rmesh", gfxRes);

var modelA = new Model(testmesh);
var modelB = new Model(testmesh);
modelB.WorldTransform = modelB.WorldTransform with { Position = new(2, 0, -0.1f), Scale = new(0.5f) };
var model173 = new Model(mesh2) { WorldTransform = new() { Position = new(0, 0, 5) } };

var now = DateTime.UtcNow;
while (window.Exists) {
    window.PumpEvents();
    if (window.MouseDelta != Vector2.Zero) {
        controller.HandleMouse(window.MouseDelta * 0.01f);
    }

    modelShader.VertexConstants.ViewMatrix /*= rMeshShader.VertexConstants.View*/ = controller.Camera.ViewMatrix;
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
    modelA.Render(commandsList, 0f);
    modelB.Render(commandsList, 0f);
    model173.Render(commandsList, 0f);
    //foreach (var meshh in aaa) {
    //    meshh.Render(commandsList);
    //}
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
