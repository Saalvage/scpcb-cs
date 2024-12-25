using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IPositionConstantMember : IConstantMember<IPositionConstantMember, Vector3> {
    public Vector3 Position { get; set; }

    Vector3 IConstantMember<IPositionConstantMember, Vector3>.Value {
        set => Position = value;
    }
}
