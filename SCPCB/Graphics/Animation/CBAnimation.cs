using System.Numerics;
using Assimp;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Graphics.Animation;

public class CBAnimation {
    private record Channel(
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

    private readonly Dictionary<string, Channel> _channels = [];

    private readonly ModelAnimationInfo _animInfo;

    private readonly float _ticksPerSecond;
    public float Duration { get; }

    public CBAnimation(ModelAnimationInfo animInfo, global::Assimp.Animation anim) {
        _animInfo = animInfo;
        foreach (var ch in anim.NodeAnimationChannels) {
            _channels[ch.NodeName] = new(
                ch.PositionKeys.Select(x => new VectorAnimationKey(x)).ToArray(),
                ch.RotationKeys.Select(x => new QuaternionAnimationKey(x)).ToArray(),
                ch.ScalingKeys.Select(x => new VectorAnimationKey(x)).ToArray());
        }
        Duration = (float)(anim.DurationInTicks / anim.TicksPerSecond);
        _ticksPerSecond = (float)anim.TicksPerSecond;
    }

    public void UpdateBones(ReadOnlySpan<IConstantHolder?> holders, float time) {
        // Seconds to ticks.
        time *= _ticksPerSecond;
        // TODO: This has actual performance implications, multiple optimizations are possible.
        // 1. Linearize node hierarchy and replace recursion with iteration.
        // 2. Apply to multiple constant holders in one traversal.
        ApplyTransforms(_animInfo.RootNode, Matrix4x4.Identity, holders);
        void ApplyTransforms(Node node, Matrix4x4 globalTransform, ReadOnlySpan<IConstantHolder> holders) {
            var local = Matrix4x4.Transpose(node.Transform);
            if (_channels.TryGetValue(node.Name, out var bone)) {
                local = bone.GetMatrix(time);
            }

            globalTransform = local * globalTransform;

            if (_animInfo.Bones.TryGetValue(node.Name, out var boneInfo)) {
                var finalMatrix = boneInfo.Offset * globalTransform;
                foreach (var holder in holders) {
                    holder?.SetArrayValue<IBoneTransformsConstantMember, Matrix4x4>(boneInfo.Id, finalMatrix);
                }
            }

            foreach (var child in node.Children) {
                ApplyTransforms(child, globalTransform, holders);
            }
        }
    }
}
