namespace SCPCB.Graphics.Shaders.ConstantMembers;

public record struct Fog(float Near, float Far) {
    public float CalculateDensity(float distance) {
        return (distance - Near) / (Far - Near);
    }
}

public interface IFogRangeConstantMember : IConstantMember<IFogRangeConstantMember, Fog> {
    public Fog Fog { get; set; }

    Fog IConstantMember<IFogRangeConstantMember, Fog>.Value {
        get => Fog;
        set => Fog = value;
    }
}
