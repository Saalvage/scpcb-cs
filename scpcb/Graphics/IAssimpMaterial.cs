using System.Numerics;
using Assimp;
using Veldrid;

namespace scpcb;

// Material that supports conversion of Assimp meshes to CB meshes.
public interface IAssimpMaterial {
    ICBMesh ConvertMesh(GraphicsDevice gfx, Mesh mesh);
}

public abstract class AssimpMaterial<TVertex> : CBMaterial<TVertex>, IAssimpMaterial where TVertex : unmanaged {
    protected AssimpMaterial(GraphicsDevice gfx, ICBShader<TVertex> shader, ResourceLayout? layout, params ICBTexture[] textures) : base(gfx, shader, layout, textures) {
        
    }

    public ICBMesh ConvertMesh(GraphicsDevice gfx, Mesh mesh) {
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

            var sv = new Model.SuperVertex {
                Position = mesh.Vertices[i].ToCS(),
                TexCoords = textureCoords,
                VertexColors = vertexColors,
                Normal = mesh.HasNormals ? mesh.Normals[i].ToCS() : Vector3.Zero,
                Tangent = mesh.HasTangentBasis ? mesh.Tangents[i].ToCS() : Vector3.Zero,
                Bitangent = mesh.HasTangentBasis ? mesh.BiTangents[i].ToCS() : Vector3.Zero,
            };
            verts[i] = ConvertVertex(sv);
        }

        return new CBMesh<TVertex>(gfx, this, verts, Array.ConvertAll(mesh.GetIndices(), Convert.ToUInt32));
    }

    protected abstract TVertex ConvertVertex(Model.SuperVertex vert);
}
