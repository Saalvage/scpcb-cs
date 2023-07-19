using System.Numerics;
using System.Reflection;
using Assimp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using scpcb;
using scpcb.Graphics;
using scpcb.RoomProviders;
using scpcb.Shaders;
using ShaderGen;
using ShaderGen.Hlsl;
using Veldrid;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

// Generate shader using ShaderGen
var compilation = CSharpCompilation.Create(null, new[] { CSharpSyntaxTree.ParseText(SourceText.From(
    """
    using ShaderGen;
    using System.Numerics;
    using static ShaderGen.ShaderBuiltins;

    [assembly: ShaderSet("Test2.MinExample", "Test2.MinExample.VertexShaderFunc", "Test2.MinExample.FragmentShaderFunc")]

    namespace Test2;

    public class MinExample
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 World;
        public Texture2DResource SurfaceTexture;
        public SamplerResource Sampler;

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [TextureCoordinateSemantic] public Vector2 TextureCoord;
        }

        public struct FragmentInput
        {
            [SystemPositionSemanticAttribute] public Vector4 Position;
            [TextureCoordinateSemantic] public Vector2 TextureCoord;
        }

        [VertexShader]
        public FragmentInput VertexShaderFunc(VertexInput input)
        {
            FragmentInput output;
            Vector4 worldPosition = Mul(World, new Vector4(input.Position, 1));
            Vector4 viewPosition = Mul(View, worldPosition);
            output.Position = Mul(Projection, viewPosition);
            output.TextureCoord = input.TextureCoord;
            return output;
        }

        [FragmentShader]
        public Vector4 FragmentShaderFunc(FragmentInput input)
        {
            return Sample(SurfaceTexture, Sampler, input.TextureCoord);
        }
    }
    """

))}, Assembly.GetEntryAssembly().GetReferencedAssemblies().AsEnumerable()
        .Concat(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "netstandard").Select(x => x.GetName()))
        .Append(typeof(Attribute).Assembly.GetName())
        .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location)),
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
var testrfff = compilation.GetDiagnostics();

var aa = Assembly.GetEntryAssembly().GetReferencedAssemblies();

var test22 = compilation.GetTypeByMetadataName("MinExample");

var backend = new HlslBackend(compilation);

var test = new ShaderGenerator(compilation, backend).GenerateShaders();

const int WIDTH = 1280;
const int HEIGHT = 720;

using var gfxRes = new GraphicsResources(WIDTH, HEIGHT);
var gfx = gfxRes.GraphicsDevice;

var factory = gfx.ResourceFactory;

using var shader2 = new UIShader(gfx);
shader2.VertexConstants.Projection = Matrix4x4.CreateOrthographic(WIDTH, HEIGHT, 0.1f, 100f);

using var coolTexture = new CBTexture(gfx, "Assets/scp.jpg");
using var mesh = new UIMesh(gfx, shader2, coolTexture);

using var commandsList = factory.CreateCommandList();

var countingTo = DateTimeOffset.Now;
var fps = 0;

var modelShader = gfxRes.ShaderCache.GetShader<ModelShader>();
var rMeshShader = gfxRes.ShaderCache.GetShader<RMeshShader>();
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
using var scp = new TestAssimpMaterial(gfx, modelShader, coolTexture);
var mesh2 = scp.ConvertMesh(gfx, scene.Meshes[0]);

modelShader.VertexConstants.View = rMeshShader.VertexConstants.View
    = Matrix4x4.CreateLookAt(new(0, 0, -5), Vector3.UnitZ, Vector3.UnitY);
modelShader.VertexConstants.Projection = rMeshShader.VertexConstants.Projection
    = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)WIDTH / HEIGHT, 0.1f, 10000f);

Veldrid.Sdl2.Sdl2Native.SDL_SetRelativeMouseMode(true);

var controller = new CharacterController();

var KeysDown = new Dictionary<Key, bool>();

var window = gfxRes.Window;
window.KeyDown += x => KeysDown[x.Key] = true;
window.KeyUp += x => KeysDown[x.Key] = false;
bool KeyDown(Key x) => KeysDown.TryGetValue(x, out var y) && y;

var r = new RMeshRoomProvider();
var aaa = r.Test("Assets/008_opt.rmesh", gfxRes);

var now = DateTime.UtcNow;
while (window.Exists) {
    window.PumpEvents();
    if (window.MouseDelta != Vector2.Zero) {
        controller.HandleMouse(window.MouseDelta * 0.01f);
    }

    modelShader.VertexConstants.View = rMeshShader.VertexConstants.View = controller.Camera.ViewMatrix;
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

    modelShader.VertexConstants.Model = rMeshShader.VertexConstants.Model
        = new Transform(new(0, 0, 0), Quaternion.CreateFromYawPitchRoll(-mesh.Position.X / 100, 0, 0), Vector3.One).GetMatrix();
    mesh.Scale.Y = (mesh.Scale.Y + delta * 10) % 5;
    mesh.Render(commandsList);
    mesh2.Render(commandsList);
    testmesh.Render(commandsList);
    foreach (var meshh in aaa) {
        meshh.Render(commandsList);
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
