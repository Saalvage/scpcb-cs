using System.Numerics;
using Assimp;
using Assimp.Unmanaged;
using BepuPhysics.Collidables;
using SCPCB.Graphics.Primitives;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Serilog.Events;
using Veldrid;
using AiMesh = Assimp.Mesh;
using Mesh = BepuPhysics.Collidables.Mesh;

namespace SCPCB.Graphics.Assimp;

// Material that supports conversion of Assimp meshes to CB meshes.
public abstract class AssimpModelLoader<TVertex> : IModelLoader where TVertex : unmanaged {
    public Scene Scene { get; }
    public string FileDir { get; }

    protected AssimpModelLoader(string file) {
        FileDir = Path.GetDirectoryName(file);
        Log.Information("Loading model {Model}", file);
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

    public (ICBShape<ConvexHull>, Vector3 OffsetFromCenter) ExtractCollisionHull(PhysicsResources physics) {
        ConvexHullHelper.CreateShape(Scene.Meshes
            .SelectMany(x => x.Vertices)
            .Select(x => x)
            .ToArray(), physics.BufferPool, out var center, out var hull);
        return (new CBShape<ConvexHull>(physics, hull), center);
    }

    public ICBShape<Mesh> ExtractCollisionMesh(PhysicsResources physics) {
        var triCount = Scene.Meshes.Sum(x => x.GetIndices().Count() / 3);
        physics.BufferPool.TakeAtLeast<Triangle>(triCount, out var triBuffer);
        foreach (var (tri, i) in Scene.Meshes.SelectMany(mesh => {
                     var inds = mesh.GetIndices().ToArray();
                     return inds.Zip(inds.Skip(1), inds.Skip(2))
                         .Select((x, i) => (x, i))
                         .Where(x => x.i % 3 == 0)
                         .Select(x => new Triangle(mesh.Vertices[x.x.Third], mesh.Vertices[x.x.Second],
                             mesh.Vertices[x.x.First]));
                 }).Select((x, i) => (x, i))) {
            triBuffer[i] = tri;
        }
        return new CBShape<Mesh>(physics, Mesh.CreateWithSweepBuild(triBuffer[..triCount], Vector3.One, physics.BufferPool));
    }

    protected virtual (TVertex[], uint[]) ConvertMesh(AiMesh mesh) {
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
