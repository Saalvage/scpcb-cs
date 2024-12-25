using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers; 

public interface IColorAlphaConstantMember : IConstantMember<IColorAlphaConstantMember, Vector4> {
    public Vector4 Color { get; set; }

    Vector4 IConstantMember<IColorAlphaConstantMember, Vector4>.Value {
        set => Color = value;
    }
}
