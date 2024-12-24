using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Graphics;

public interface IMeshMaterial {
    ICBMesh Mesh { get; }
    ICBMaterial Material { get; }

    void Deconstruct(out ICBMesh mesh, out ICBMaterial material) => (mesh, material) = (Mesh, Material);
}

public interface IMeshMaterial<TVertex> : IMeshMaterial where TVertex : unmanaged {
    ICBMesh IMeshMaterial.Mesh => Mesh;
    ICBMesh<TVertex> Mesh { get; }
    
    ICBMaterial IMeshMaterial.Material => Material;
    ICBMaterial<TVertex> Material { get; }

    void IMeshMaterial.Deconstruct(out ICBMesh mesh, out ICBMaterial material) {
        Deconstruct(out var vMesh, out var vMat);
        (mesh, material) = (vMesh, vMat);
    }

    void Deconstruct(out ICBMesh<TVertex> mesh, out ICBMaterial<TVertex> material);
}

public record struct MeshMaterial<TVertex>(ICBMesh<TVertex> Mesh, ICBMaterial<TVertex> Material)
    : IMeshMaterial<TVertex> where TVertex : unmanaged;

public static class MeshMaterialExtensions {
    public static IEnumerable<ICBModel<TVertex>> Instantiate<TVertex>(this IEnumerable<IMeshMaterial<TVertex>> meshMats) where TVertex : unmanaged
        => meshMats.Cast<IMeshMaterial>().Instantiate().Cast<ICBModel<TVertex>>();

    public static IEnumerable<ICBModel> Instantiate(this IEnumerable<IMeshMaterial> meshMats) {
        var dic = new Dictionary<ICBShader, IConstantHolder?>();

        // TODO: I still don't like how constants are handled in models..
        foreach (var mm in meshMats) {
            var (mesh, mat) = mm;
            var constants = dic.TryGetValue(mat.Shader, out var val) ? val
                : dic[mat.Shader] = mat.Shader.TryCreateInstanceConstants();

            yield return mesh.CreateModel(mat, constants);
        }
    }
}
