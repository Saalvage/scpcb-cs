using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers; 

public interface IViewPositionConstantMember : IConstantMember<IViewPositionConstantMember, Vector3> {
    public Vector3 ViewPosition { get; set; }

    Vector3 IConstantMember<IViewPositionConstantMember, Vector3>.Value {
        set => ViewPosition = value;
    }
}
