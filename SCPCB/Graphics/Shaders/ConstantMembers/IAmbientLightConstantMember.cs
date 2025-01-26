namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IAmbientLightConstantMember : IConstantMember<IAmbientLightConstantMember, float> {
    public float AmbientLightLevel { get; set; }

    float IConstantMember<IAmbientLightConstantMember, float>.Value {
        get => AmbientLightLevel;
        set => AmbientLightLevel = value;
    }
}

