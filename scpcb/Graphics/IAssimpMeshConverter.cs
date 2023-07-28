using System.Numerics;
using Assimp;
using Veldrid;

namespace scpcb.Graphics;

// Material that supports conversion of Assimp meshes to CB meshes.
public interface IAssimpMeshConverter<TVertex> where TVertex : unmanaged {
    ICBMesh ConvertMesh(GraphicsDevice gfx, Mesh mesh, ICBMaterial<TVertex> mat);
}

public abstract class AssimpMeshConverter<TVertex> : IAssimpMeshConverter<TVertex> where TVertex : unmanaged {
    public ICBMesh ConvertMesh(GraphicsDevice gfx, Mesh mesh, ICBMaterial<TVertex> mat) {
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
                Position = mesh.Vertices[i].ToCS(),
                TexCoords = textureCoords,
                VertexColors = vertexColors,
                Normal = mesh.HasNormals ? mesh.Normals[i].ToCS() : Vector3.Zero,
                Tangent = mesh.HasTangentBasis ? mesh.Tangents[i].ToCS() : Vector3.Zero,
                Bitangent = mesh.HasTangentBasis ? mesh.BiTangents[i].ToCS() : Vector3.Zero,
            };
            verts[i] = ConvertVertex(sv);
        }

        return new CBMesh<TVertex>(gfx, mat, verts, Array.ConvertAll(mesh.GetIndices(), Convert.ToUInt32));
    }

    protected abstract TVertex ConvertVertex(AssimpVertex vert);
}
