using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers; 

public interface IColorConstantMember : IConstantMember<IColorConstantMember, Vector3> {
    public Vector3 Color { get; set; }

    Vector3 IConstantMember<IColorConstantMember, Vector3>.Value {
        get => Color;
        set => Color = value;
    }
}
