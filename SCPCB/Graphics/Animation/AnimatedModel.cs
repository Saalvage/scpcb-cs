using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Graphics.Animation;

public class AnimatedModel : Model, IConstantProvider, IUpdatable {
    private CBAnimation? _animation;
    public CBAnimation? Animation {
        get => _animation;
        set {
            _animation = value;
            Time = 0;
        }
    }

    public float Time { get; set; }
    public float Speed { get; set; } = 1;
    public bool IsPaused { get; set; }
    public bool Looping { get; set; }

    private readonly IAnimatedModelTemplate _template;

    public AnimatedModel(IAnimatedModelTemplate template) : base(template.BaseTemplate) {
        _template = template;
    }

    public void ApplyTo(ReadOnlySpan<IConstantHolder?> holders, float interp) {
        if (Animation == null) {
            for (var i = 0; i < _template.Info.Bones.Count; i++) {
                foreach (var holder in holders) {
                    holder?.SetArrayValue<IBoneTransformsConstantMember, Matrix4x4>(i, Matrix4x4.Identity);
                }
            }
        } else {
            Animation.UpdateBones(holders, Time);
        }
        
        ((IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>)this).ApplyToInternal(holders, interp);
    }

    public void Update(float delta) {
        if (Animation != null && !IsPaused) {
            Time += Speed * delta;
            if (Time > Animation.Duration) {
                Time = Looping ? Time % Animation.Duration : Animation.Duration;
            }
        }
    }
}
