using System.Numerics;

namespace scpcb.Graphics.Shaders.ConstantMembers; 

public interface IColorConstantMember : IConstantMember<IColorConstantMember, Vector3> {
    public Vector3 Color { get; set; }

    Vector3 IConstantMember<IColorConstantMember, Vector3>.Value {
        set => Color = value;
    }
}
