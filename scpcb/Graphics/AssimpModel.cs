using System.Numerics;
using Assimp;
using Veldrid;

namespace scpcb.Graphics;

public ref struct AssimpVertex {
    public Vector3 Position;
    public Span<Vector3> TexCoords;
    public Span<Vector4> VertexColors;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Bitangent;
}

public class AssimpModel<TVertex> where TVertex : unmanaged {
    private ICBMesh<TVertex>[] _meshes;

    public AssimpModel(GraphicsDevice gfx, Func<Material, ICBMaterial<TVertex>> matConverter, string file) {
        using var assimp = new AssimpContext();
        var scene = assimp.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality);
        var mats = scene.Materials.Select(matConverter).ToArray();
        //_meshes = scene.Meshes.Select(x => mats[x.MaterialIndex].ConvertMesh(gfx, x)).ToArray();
    }
}