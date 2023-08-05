using System.Numerics;
using Assimp;
using BepuPhysics;
using BepuPhysics.Collidables;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Physics;
using scpcb.Utility;
using Veldrid;
using Mesh = Assimp.Mesh;

namespace scpcb.Graphics.Assimp;

// Material that supports conversion of Assimp meshes to CB meshes.
public interface IAssimpMeshConverter<TVertex> where TVertex : unmanaged {
    ICBModel<TVertex> ConvertMesh(GraphicsDevice gfx, Mesh mesh, ICBMaterial<TVertex> mat, Vector3 middle);
    ConvexHull ConvertToConvexHull(PhysicsResources physics, IEnumerable<Mesh> meshes, out Vector3 offset);
    (ICBModel<TVertex>[], ConvexHull) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
    PhysicsModelCollection CreateModel(GraphicsDevice gfx, PhysicsResources physics, string file);
}

public abstract class AssimpMeshConverter<TVertex> : IAssimpMeshConverter<TVertex> where TVertex : unmanaged {
    public ICBModel<TVertex> ConvertMesh(GraphicsDevice gfx, Mesh mesh, ICBMaterial<TVertex> mat, Vector3 middle) {
        // TODO: Upper limit to stackalloc size
        Span<Vector3> textureCoords = stackalloc Vector3[mesh.TextureCoordinateChannelCount];
        Span<Vector4> vertexColors = stackalloc Vector4[mesh.VertexColorChannelCount];

        var verts = new TVertex[mesh.VertexCount];

        for (var i = 0; i < mesh.VertexCount; i++) {
            for (var j = 0; j < mesh.TextureCoordinateChannelCount; j++) {
                textureCoords[j] = mesh.TextureCoordinateChannels[j][i].ToCS();
            }
            for (var j = 0; j < mesh.VertexColorChannelCount; j++) {
                vertexColors[j] = mesh.VertexColorChannels[j][i].ToCS();
            }

            var sv = new AssimpVertex {
                Position = mesh.Vertices[i].ToCS() / 10 - middle, // TODO: Better way to handle this :(
                TexCoords = textureCoords,
                VertexColors = vertexColors,
                Normal = mesh.HasNormals ? mesh.Normals[i].ToCS() : Vector3.Zero,
                Tangent = mesh.HasTangentBasis ? mesh.Tangents[i].ToCS() : Vector3.Zero,
                Bitangent = mesh.HasTangentBasis ? mesh.BiTangents[i].ToCS() : Vector3.Zero,
            };
            verts[i] = ConvertVertex(sv);
        }

        // TODO: Share constants?
        return new CBModel<TVertex>(mat.Shader.TryCreateInstanceConstants(), mat, new CBMesh<TVertex>(gfx, verts, Array.ConvertAll(mesh.GetIndices(), Convert.ToUInt32)));
    }

    public ConvexHull ConvertToConvexHull(PhysicsResources physics, IEnumerable<Mesh> meshes, out Vector3 offset) {
        ConvexHullHelper.CreateShape(meshes
            .SelectMany(x => x.Vertices)
            .Select(x => x.ToCS() / 10)
            .ToArray(), physics.BufferPool, out offset, out var hull);
        return hull;
    }

    public (ICBModel<TVertex>[], ConvexHull) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file) {
        using var assimp = new AssimpContext();
        var scene = assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality);
        var hull = ConvertToConvexHull(physics, scene.Meshes, out var middle);
        var mats = scene.Materials.Select(ConvertMaterial).ToArray();
        return (scene.Meshes.Select(x => ConvertMesh(gfx, x, mats[x.MaterialIndex], middle)).ToArray(), hull);
    }

    public PhysicsModelCollection CreateModel(GraphicsDevice gfx, PhysicsResources physics, string file) {
        var (meshes, hull) = LoadMeshes(gfx, physics, file);
        var hullId = physics.Simulation.Shapes.Add(hull);
        return new(physics, physics.Simulation.Bodies.GetBodyReference(physics.Simulation.Bodies.Add(BodyDescription.CreateDynamic(RigidPose.Identity, hull.ComputeInertia(1), hullId, 0.01f))), meshes);
    }

    protected abstract TVertex ConvertVertex(AssimpVertex vert);
    protected abstract ICBMaterial<TVertex> ConvertMaterial(Material mat);
}
