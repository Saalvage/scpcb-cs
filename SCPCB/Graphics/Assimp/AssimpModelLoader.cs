using System.Numerics;
using Assimp;
using Assimp.Unmanaged;
using BepuPhysics.Collidables;
using SCPCB.Graphics.ModelTemplates;
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

    public ICBMesh<TVertex> ConvertMesh(GraphicsDevice gfx, Mesh mesh) {
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
        PostMutateVertices(mesh, verts);

        return new CBMesh<TVertex>(gfx, verts, mesh.GetUnsignedIndices().ToArray());
    }

    /// <summary>
    /// Allows a derived class to mutate the vertices after being loaded and before being turned into a mesh.
    /// <remarks>
    /// Intended use case if for e.g. attaching associated bone IDs and weights to vertices which are stored in the bones themselves.
    /// </remarks>
    /// </summary>
    protected virtual void PostMutateVertices(Mesh mesh, TVertex[] vertices) { }

    public virtual ICBShape<ConvexHull> ConvertToConvexHull(PhysicsResources physics, IEnumerable<Mesh> meshes, out Vector3 center) {
        ConvexHullHelper.CreateShape(meshes
            .SelectMany(x => x.Vertices)
            .Select(x => x)
            .ToArray(), physics.BufferPool, out center, out var hull);
        return new CBShape<ConvexHull>(physics, hull);
    }

    protected Scene LoadScene(string file) {
        using var assimp = new AssimpContext();
        SerilogLogger.Instance.ModelFile = file;
        return assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.FlipUVs);
    }

    public OwningModelTemplate LoadMeshes(GraphicsDevice gfx, string file) {
        var scene = LoadScene(file);
        var fileDir = Path.GetDirectoryName(file);
        var mats = scene.Materials.Select(x => ConvertMaterial(x, fileDir)).ToArray();
        return new(scene.Meshes.Select(
            IMeshMaterial (x) => new MeshMaterial<TVertex>(ConvertMesh(gfx, x),
                mats[x.MaterialIndex])).ToArray());
    }

    public OwningPhysicsModelTemplate LoadMeshesWithCollision(GraphicsDevice gfx, PhysicsResources physics, string file) {
        var scene = LoadScene(file);
        var hull = ConvertToConvexHull(physics, scene.Meshes, out var center);
        var fileDir = Path.GetDirectoryName(file);
        var mats = scene.Materials.Select(x => ConvertMaterial(x, fileDir)).ToArray();
        return new(scene.Meshes.Select(
            IMeshMaterial (x) => new MeshMaterial<TVertex>(ConvertMesh(gfx, x),
                mats[x.MaterialIndex])).ToArray(), hull, center);
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
