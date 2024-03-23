using System.Numerics;

namespace scpcb.Graphics.Shaders.ConstantMembers;

public interface IUIScaleConstantMember : IConstantMember<IUIScaleConstantMember, Vector2> {
    public Vector2 Scale { get; set; }

    Vector2 IConstantMember<IUIScaleConstantMember, Vector2>.Value {
        set => Scale = value;
    }
}
