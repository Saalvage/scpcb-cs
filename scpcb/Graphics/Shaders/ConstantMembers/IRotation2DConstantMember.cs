using System.Numerics;

namespace scpcb.Graphics.Shaders.ConstantMembers;

public interface IRotation2DConstantMember : IConstantMember<IRotation2DConstantMember, Vector2> {
    public Vector2 SinCosDeg { get; set; }

    Vector2 IConstantMember<IRotation2DConstantMember, Vector2>.Value {
        set => SinCosDeg = value;
    }
}
