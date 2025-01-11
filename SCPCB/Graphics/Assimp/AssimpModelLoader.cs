using System.Numerics;
using Assimp;
using Assimp.Unmanaged;
using BepuPhysics.Collidables;
using SCPCB.Graphics.Primitives;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Serilog.Events;
using Veldrid;
using Mesh = Assimp.Mesh;

namespace SCPCB.Graphics.Assimp;

// Material that supports conversion of Assimp meshes to CB meshes.
public abstract class AssimpModelLoader<TVertex> : IModelLoader where TVertex : unmanaged {
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

    public Scene Scene { get; }
    public string FileDir { get; }

    protected AssimpModelLoader(string file) {
        FileDir = Path.GetDirectoryName(file);
        using var assimp = new AssimpContext();
        SerilogLogger.Instance.ModelFile = file;
        Scene = assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs);
    }

    public IReadOnlyList<IMeshMaterial> ExtractMeshes(GraphicsDevice gfx) {
        var mats = Scene.Materials.Select(x => ConvertMaterial(x, FileDir)).ToArray();
        return Scene.Meshes.Select(
            IMeshMaterial (x) => {
                var (vertices, indices) = ConvertMesh(x);
                return new MeshMaterial<TVertex>(new CBMesh<TVertex>(gfx, vertices, indices),
                    mats[x.MaterialIndex]);
            }).ToArray();
    }

    public (ICBShape, Vector3 OffsetFromCenter) ExtractCollisionShape(PhysicsResources physics) {
        ConvexHullHelper.CreateShape(Scene.Meshes
            .SelectMany(x => x.Vertices)
            .Select(x => x)
            .ToArray(), physics.BufferPool, out var center, out var hull);
        return (new CBShape<ConvexHull>(physics, hull), center);
    }

    protected virtual (TVertex[], uint[]) ConvertMesh(Mesh mesh) {
        if (mesh.TextureCoordinateChannelCount > AiDefines.AI_MAX_NUMBER_OF_TEXTURECOORDS) {
            throw new ArgumentException("Abnormal amount of texture coordinate channels!", nameof(mesh));
        }

        if (mesh.VertexColorChannelCount > AiDefines.AI_MAX_NUMBER_OF_COLOR_SETS) {
            throw new ArgumentException("Abnormal amount of vertex color sets!", nameof(mesh));
        }

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
                Position = mesh.Vertices[i],
                TexCoords = textureCoords,
                VertexColors = vertexColors,
                Normal = mesh.HasNormals ? mesh.Normals[i] : Vector3.Zero,
                Tangent = mesh.HasTangentBasis ? mesh.Tangents[i] : Vector3.Zero,
                Bitangent = mesh.HasTangentBasis ? mesh.BiTangents[i] : Vector3.Zero,
            };
            verts[i] = ConvertVertex(sv);
        }

        return (verts, mesh.GetUnsignedIndices().ToArray());
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
public class AutomaticAssimpModelLoader<TShader, TVertex, TPlugin> : AssimpModelLoader<TVertex>
        where TShader : IAssimpMaterialConvertible<TVertex, TPlugin>
        where TVertex : unmanaged, IAssimpVertexConvertible<TVertex> {
    private readonly TPlugin _plugin;

    public AutomaticAssimpModelLoader(TPlugin plugin, string file) : base(file) {
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

    public PluginAssimpModelLoader(VertexConverter vertexConverter, Func<Material, string, ICBMaterial<TVertex>> materialConverter, string file)
        : base(file) {
        _vertexConverter = vertexConverter;
        _materialConverter = materialConverter;
    }

    protected override TVertex ConvertVertex(AssimpVertex vert) => _vertexConverter(vert);

    protected override ICBMaterial<TVertex> ConvertMaterial(Material mat, string fileDir) => _materialConverter(mat, fileDir);
}
