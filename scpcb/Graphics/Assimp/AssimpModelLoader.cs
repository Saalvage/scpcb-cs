using System.Numerics;
using Assimp;
using BepuPhysics.Collidables;
using scpcb.Graphics.Primitives;
using scpcb.Physics;
using scpcb.Physics.Primitives;
using Serilog.Events;
using Veldrid;
using Mesh = Assimp.Mesh;

namespace scpcb.Graphics.Assimp;

// Material that supports conversion of Assimp meshes to CB meshes.
public abstract class AssimpModelLoader<TVertex> : IModelLoader<TVertex> where TVertex : unmanaged {
    // I'm not sure if this can be implemented any better, it seems like the C loggers have no access to explicit severity.
    private class SerilogLogger : LogStream {
        public static SerilogLogger Instance { get; } = new();

        // This breaks when models are loaded in parallel, but there's no solution to that really.
        public string ModelFile { get; set; }

        protected override void Dispose(bool disposing) {
            Detach();
            base.Dispose(disposing);
        }

        protected override void LogMessage(string msg, string userData) {
            const string SPLITTER = ", ";
            var splitterIndex = Math.Max(0, msg.IndexOf(SPLITTER));
            var severity = msg[..splitterIndex];
            Serilog.Log.Write(severity switch {
                // We downgrade their severity because Assimp is yapping too much.
                "Debug" => LogEventLevel.Verbose,
                "Info" => LogEventLevel.Debug,
                "Warn" => LogEventLevel.Warning,
                "Error" => LogEventLevel.Error,
                _ => LogEventLevel.Fatal,
            }, "Assimp ({Model}) {AssimpLog}", ModelFile, msg[(splitterIndex+SPLITTER.Length)..].Trim());
        }
    }

    static AssimpModelLoader() {
        SerilogLogger.Instance.Attach();
    }

    public MeshMaterial<TVertex> ConvertMesh(GraphicsDevice gfx, Mesh mesh, ICBMaterial<TVertex> mat, Vector3 center) {
        // TODO: Upper limit to stackalloc size
        Span<Vector3> textureCoords = stackalloc Vector3[mesh.TextureCoordinateChannelCount];
        Span<Vector4> vertexColors = stackalloc Vector4[mesh.VertexColorChannelCount];

        var verts = new TVertex[mesh.VertexCount];

        for (var i = 0; i < mesh.VertexCount; i++) {
            for (var j = 0; j < mesh.TextureCoordinateChannelCount; j++) {
                textureCoords[j] = mesh.TextureCoordinateChannels[j][i];
            }
            for (var j = 0; j < mesh.VertexColorChannelCount; j++) {
                vertexColors[j] = mesh.VertexColorChannels[j][i];
            }

            var sv = new AssimpVertex {
                Position = mesh.Vertices[i] / 10 - center, // TODO: Better way to handle this :(
                TexCoords = textureCoords,
                VertexColors = vertexColors,
                Normal = mesh.HasNormals ? mesh.Normals[i] : Vector3.Zero,
                Tangent = mesh.HasTangentBasis ? mesh.Tangents[i] : Vector3.Zero,
                Bitangent = mesh.HasTangentBasis ? mesh.BiTangents[i] : Vector3.Zero,
            };
            verts[i] = ConvertVertex(sv);
        }

        return new(new CBMesh<TVertex>(gfx, verts, mesh.GetUnsignedIndices().ToArray()), mat);
    }

    public ICBShape<ConvexHull> ConvertToConvexHull(PhysicsResources physics, IEnumerable<Mesh> meshes, out Vector3 center) {
        ConvexHullHelper.CreateShape(meshes
            .SelectMany(x => x.Vertices)
            .Select(x => x / 10)
            .ToArray(), physics.BufferPool, out center, out var hull);
        return new CBShape<ConvexHull>(physics.Simulation, hull);
    }

    public (IMeshMaterial<TVertex>[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file) {
        using var assimp = new AssimpContext();
        SerilogLogger.Instance.ModelFile = file;
        var fileDir = Path.GetDirectoryName(file);
        var scene = assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs);
        var hull = ConvertToConvexHull(physics, scene.Meshes, out var center);
        var mats = scene.Materials.Select(x => ConvertMaterial(x, fileDir)).ToArray();
        return (scene.Meshes.Select(x => (IMeshMaterial<TVertex>)ConvertMesh(gfx, x, mats[x.MaterialIndex], center)).ToArray(), hull, center);
    }

    protected abstract TVertex ConvertVertex(AssimpVertex vert);
    protected abstract ICBMaterial<TVertex> ConvertMaterial(Material mat, string fileDir);
}

/// <summary>
/// Intended for shaders which you can directly edit.
/// </summary>
/// <typeparam name="TShader"></typeparam>
/// <typeparam name="TVertex"></typeparam>
/// <typeparam name="TPlugin"></typeparam>
public sealed class AutomaticAssimpModelLoader<TShader, TVertex, TPlugin> : AssimpModelLoader<TVertex>
        where TShader : IAssimpMaterialConvertible<TVertex, TPlugin>
        where TVertex : unmanaged, IAssimpVertexConvertible<TVertex> {
    private readonly TPlugin _plugin;

    public AutomaticAssimpModelLoader(TPlugin plugin) {
        _plugin = plugin;
    }

    protected override TVertex ConvertVertex(AssimpVertex vert) => TVertex.ConvertVertex(vert);

    protected override ICBMaterial<TVertex> ConvertMaterial(Material mat, string fileDir) => TShader.ConvertMaterial(mat, fileDir, _plugin);
}

/// <summary>
/// Intended for shaders which you can not directly edit.
/// </summary>
/// <typeparam name="TVertex"></typeparam>
public sealed class PluginAssimpModelLoader<TVertex> : AssimpModelLoader<TVertex> where TVertex : unmanaged {
    public delegate TVertex VertexConverter(AssimpVertex vert);

    private readonly VertexConverter _vertexConverter;
    private readonly Func<Material, string, ICBMaterial<TVertex>> _materialConverter;

    public PluginAssimpModelLoader(VertexConverter vertexConverter, Func<Material, string, ICBMaterial<TVertex>> materialConverter) {
        _vertexConverter = vertexConverter;
        _materialConverter = materialConverter;
    }

    protected override TVertex ConvertVertex(AssimpVertex vert) => _vertexConverter(vert);

    protected override ICBMaterial<TVertex> ConvertMaterial(Material mat, string fileDir) => _materialConverter(mat, fileDir);
}
