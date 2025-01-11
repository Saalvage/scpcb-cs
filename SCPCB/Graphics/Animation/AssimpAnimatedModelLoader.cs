using SCPCB.Graphics.Assimp;
using System.Diagnostics;
using System.Numerics;
using SCPCB.Graphics.Shaders.Vertices;
using Veldrid;
using Mesh = Assimp.Mesh;

namespace SCPCB.Graphics.Animation;

public class AssimpAnimatedModelLoader<TShader, TVertex, TPlugin> : AutomaticAssimpModelLoader<TShader, TVertex, TPlugin>, IAnimatedModelLoader
    where TShader : IAssimpMaterialConvertible<TVertex, TPlugin>
    where TVertex : unmanaged, IAnimatedVertex, IAssimpVertexConvertible<TVertex> {

    private readonly Dictionary<string, BoneInfo> _boneInfos = [];

    public AssimpAnimatedModelLoader(TPlugin plugin, string file) : base(plugin, file) { }

    protected override (TVertex[], uint[]) ConvertMesh(Mesh mesh) {
        var (vertices, indices) = base.ConvertMesh(mesh);
        foreach (var bone in mesh.Bones) {
            // TODO: We're putting all bones of the scene in one dictionary.
            // It would be possible to have one dictionary per mesh, with only the bones affected by that mesh.
            // I'm unsure which is more performant, with one dictionary the meshes may share one constant holder,
            // but they have to allocate space for bones they might not possess, which might also push them over the bone limit.
            // The decision to go with one dictionary has been made out of the practical reason that
            // all meshes receive the same constant holder in the current implementation and there
            // is no way to differentiate between meshes when applying constant buffers.
            // P.S.: An additional issue with the used approach might be bones with duplicate names.
            if (!_boneInfos.TryGetValue(bone.Name, out var info)) {
                info = new(_boneInfos.Count, Matrix4x4.Transpose(bone.OffsetMatrix));
                _boneInfos.Add(bone.Name, info);
            }
            var boneIndex = info.Id;
            foreach (var weight in bone.VertexWeights) {
                int i;
                for (i = 0; i < 4; i++) {
                    ref var v = ref vertices[weight.VertexID];
                    if (v.BoneIDs[i] < 0) {
                        var boneID = v.BoneIDs;
                        boneID[i] = boneIndex;
                        v.BoneIDs = boneID;
                        var boneWeight = v.BoneWeights;
                        boneWeight[i] = weight.Weight;
                        v.BoneWeights = boneWeight;
                        break;
                    }
                }
                Debug.Assert(i < 4, "Too many weights for one bone!");
            }
        }
        return (vertices, indices);
    }

    public OwningAnimatedModelTemplate LoadAnimatedModel(GraphicsDevice gfx) {
        var meshes = ExtractMeshes(gfx);
        var animationInfo = new ModelAnimationInfo(_boneInfos, Scene.RootNode);
        var animations = Scene.Animations.ToDictionary(x => x.Name, x => new CBAnimation(animationInfo, x));
        return new(animationInfo, animations, new(meshes));
    }
}
