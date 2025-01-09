using SCPCB.Graphics.Assimp;
using System.Diagnostics;
using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Mesh = Assimp.Mesh;

namespace SCPCB.Graphics.Animation;

public class AssimpAnimatedModelLoader<TShader, TVertex, TPlugin> : AutomaticAssimpModelLoader<TShader, TVertex, TPlugin>
    where TShader : IAssimpMaterialConvertible<TVertex, TPlugin>
    where TVertex : unmanaged, IAnimatedVertex, IAssimpVertexConvertible<TVertex> {

    // TODO: Doing this stateful like this SUCKS!
    private Dictionary<string, BoneInfo> _currBones;

    public AssimpAnimatedModelLoader(TPlugin plugin) : base(plugin) { }

    public (CBAnimatedModelTemplate<TVertex>, Dictionary<string, CBAnimation>) LoadAnimatedMeshes(GraphicsResources gfxRes, string file) {
        var scene = LoadScene(file);
        var fileDir = Path.GetDirectoryName(file);
        var boneInfos = new Dictionary<string, BoneInfo>[scene.MeshCount];
        var meshMaterials = Enumerable.Range(0, scene.MeshCount).Select(MakeMeshMaterial).ToArray();
        var animationInfo = new CBAnimatedModelTemplate<TVertex>(boneInfos, scene.RootNode, meshMaterials);
        var animations = scene.Animations.ToDictionary(x => x.Name, x => new CBAnimation(animationInfo, x));
        return (animationInfo, animations);

        MeshMaterial<TVertex> MakeMeshMaterial(int index) {
            var mesh = scene.Meshes[index];
            _currBones = boneInfos[index] = new();
            return new(ConvertMesh(gfxRes.GraphicsDevice, mesh),
                ConvertMaterial(scene.Materials[mesh.MaterialIndex], fileDir));
        }
    }

    // We don't want a convex hull for animated meshes since it would be of little use.
    // TODO: Look into potentially generating compound convex hulls for empties which could be animated with the mesh.
    public override ICBShape<ConvexHull>? ConvertToConvexHull(PhysicsResources physics, IEnumerable<Mesh> meshes, out Vector3 center) {
        center = Vector3.Zero;
        return null;
    }

    protected override void PostMutateVertices(Mesh mesh, TVertex[] vertices) {
        foreach (var (bone, boneIndex) in mesh.Bones.Select((x, i) => (x, i))) {
            _currBones.Add(bone.Name, new(boneIndex, Matrix4x4.Transpose(bone.OffsetMatrix)));
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
    }
}
