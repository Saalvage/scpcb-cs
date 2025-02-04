using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IScale2DConstantMember : IConstantMember<IScale2DConstantMember, Vector2> {
    public Vector2 Scale { get; set; }

    Vector2 IConstantMember<IScale2DConstantMember, Vector2>.Value {
        get => Scale;
        set => Scale = value;
    }
}
