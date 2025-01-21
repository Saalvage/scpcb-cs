namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IBlurStrengthConstantMember : IConstantMember<IBlurStrengthConstantMember, float> {
    public float BlurStrength { get; set; }

    float IConstantMember<IBlurStrengthConstantMember, float>.Value {
        get => BlurStrength;
        set => BlurStrength = value;
    }
}

