using System.Numerics;
using Assimp;
using SCPCB.Graphics.Primitives;

namespace SCPCB.Graphics.Animation;

public record BoneInfo(int Id, Matrix4x4 Offset);

public interface ICBAnimatedModelTemplate {
    Dictionary<string, BoneInfo>[] BonesPerMesh { get; }
    Node RootNode { get; }
    IReadOnlyList<ICBModel> Instantiate();
}

public record CBAnimatedModelTemplate<TVertex>(Dictionary<string, BoneInfo>[] BonesPerMesh, Node RootNode, MeshMaterial<TVertex>[] Meshes)
        : ICBAnimatedModelTemplate
        where TVertex : unmanaged {
    IReadOnlyList<ICBModel> ICBAnimatedModelTemplate.Instantiate() => Instantiate();
    public CBModel<TVertex>[] Instantiate()
        // TODO: Ability to share constants among meshes.
        => Meshes.Select(x => new CBModel<TVertex>(x.Material.Shader.TryCreateInstanceConstants(), x.Material, x.Mesh))
            .ToArray();
}
