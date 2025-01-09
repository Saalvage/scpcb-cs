using System.Numerics;
using Assimp;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Graphics.Animation;

public class CBAnimation {
    private record Bone(
        VectorAnimationKey[] PositionKeys,
        QuaternionAnimationKey[] RotationKeys,
        VectorAnimationKey[] ScalingKeys) {

        public Matrix4x4 GetMatrix(float time) {
            var scale = VectorAnimationKey.CalculateInterpolatedValue(ScalingKeys, time);
            var rot = QuaternionAnimationKey.CalculateInterpolatedValue(RotationKeys, time);
            var pos = VectorAnimationKey.CalculateInterpolatedValue(PositionKeys, time);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(pos);
        }
    }

    private readonly Dictionary<string, Bone> _bones = [];

    private readonly ICBAnimatedModelTemplate _modelTemplate;

    public CBAnimation(ICBAnimatedModelTemplate modelTemplate, global::Assimp.Animation anim) {
        _modelTemplate = modelTemplate;
        foreach (var ch in anim.NodeAnimationChannels) {
            _bones[ch.NodeName] = new(
                ch.PositionKeys.Select(x => new VectorAnimationKey(x)).ToArray(),
                ch.RotationKeys.Select(x => new QuaternionAnimationKey(x)).ToArray(),
                ch.ScalingKeys.Select(x => new VectorAnimationKey(x)).ToArray());
        }
    }

    public void UpdateBones(IConstantHolder holder, int index, float time) {
        // TODO: This has actual performance implications, multiple optimizations are possible.
        // 1. Linearize node hierarchy and replace recursion with iteration.
        // 2. Apply to multiple constant holders in one traversal.
        ApplyTransforms(_modelTemplate.RootNode, Matrix4x4.Identity);
        void ApplyTransforms(Node node, Matrix4x4 globalTransform) {
            var local = Matrix4x4.Transpose(node.Transform);
            if (_bones.TryGetValue(node.Name, out var bone)) {
                local = bone.GetMatrix(time);
            }

            globalTransform = local * globalTransform;

            if (_modelTemplate.BonesPerMesh[index].TryGetValue(node.Name, out var boneInfo)) {
                holder.SetArrayValue<IBoneTransformsConstantMember, Matrix4x4>(boneInfo.Offset * globalTransform, boneInfo.Id);
            }

            foreach (var child in node.Children) {
                ApplyTransforms(child, globalTransform);
            }
        }
    }
}
