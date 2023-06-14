using System.Numerics;
using Assimp;
using Veldrid;

namespace scpcb;

public class Model {
    public ref struct SuperVertex {
        public Vector3 Position;
        public Span<Vector3> TexCoords;
        public Span<Vector4> VertexColors;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;
    }
    
    private ICBMesh[] _meshes;

    public Model(GraphicsDevice gfx, Func<Material, IAssimpMaterial> matConverter, string file) {
        using var assimp = new AssimpContext();
        var scene = assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality);
        var mats = scene.Materials.Select(matConverter).ToArray();
        _meshes = scene.Meshes.Select(x => mats[x.MaterialIndex].ConvertMesh(gfx, x)).ToArray();
    }
}