using System.Numerics;
using scpcb;
using scpcb.Graphics;
using scpcb.Shaders;
using Veldrid;
using Veldrid.StartupUtilities;

const int WIDTH = 1280;
const int HEIGHT = 720;

VeldridStartup.CreateWindowAndGraphicsDevice(new() {
    WindowWidth = WIDTH,
    WindowHeight = HEIGHT,
    X = 100,
    Y = 100,
    WindowTitle = "SCP-087-B",
}, new() {
    Debug = true,
    PreferStandardClipSpaceYDirection = true,
    PreferDepthRangeZeroToOne = true,
}, out var window, out var _gfx);
window.CursorVisible = false;

using var gfx = _gfx;

var factory = gfx.ResourceFactory;

using var shader2 = new UIShader(gfx);
shader2.VertexConstants.Projection = Matrix4x4.CreateOrthographic(WIDTH, HEIGHT, 0.1f, 100f);

using var coolTexture = new CBTexture(gfx, "Assets/scp.jpg");
using var mesh = new UIMesh(gfx, shader2, coolTexture);

using var commandsList = factory.CreateCommandList();

var countingTo = DateTimeOffset.Now;
var fps = 0;

using var modelShader = new ModelShader(gfx);
using var testmesh = new CBMesh<ModelShader.Vertex>(gfx, modelShader.CreateMaterial(coolTexture),
    new ModelShader.Vertex[] {
        new(new(-1f, 1f, 0), new(1, 0)),
        new(new(1f, 1f, 0), new(0, 0)),
        new(new(-1f, -1f, 0), new(1, 1)),
        new(new(1f, -1f, 0), new(0, 1)),
    },
    new ushort[] { 2, 1, 0, 1, 2, 3 });

using var scp = new TestAssimpMaterial();

modelShader.VertexConstants.View = Matrix4x4.CreateLookAt(new(0, 0, -5), Vector3.UnitZ, Vector3.UnitY);
modelShader.VertexConstants.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)WIDTH / HEIGHT, 0.1f, 10000f);

Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

var controller = new CharacterController();

var KeysDown = new Dictionary<Key, bool>();

window.KeyDown += x => KeysDown[x.Key] = true;
window.KeyUp += x => KeysDown[x.Key] = false;
bool KeyDown(Key x) => KeysDown.TryGetValue(x, out var y) && y;

var now = DateTime.UtcNow;
while (window.Exists) {
    window.PumpEvents();
    if (window.MouseDelta != Vector2.Zero) {
        controller.HandleMouse(window.MouseDelta * 0.01f);
    }

    modelShader.VertexConstants.View = controller.Camera.ViewMatrix;
    commandsList.Begin();
    commandsList.SetFramebuffer(gfx.SwapchainFramebuffer);
    commandsList.ClearColorTarget(0, RgbaFloat.Black);
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
    
    modelShader.VertexConstants.Model = new Transform(new(0, 0, 0), Quaternion.CreateFromYawPitchRoll(-mesh.Position.X / 100, 0, 0), Vector3.One).GetMatrix();
    mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
    mesh.Render(commandsList);
    testmesh.Render(commandsList);
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
