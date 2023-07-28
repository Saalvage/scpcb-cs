using System.Numerics;

namespace scpcb.Graphics.Shaders.ConstantMembers; 

public interface IWorldMatrixConstantMember : IConstantMember<Matrix4x4> {
    public Matrix4x4 WorldMatrix { get; set; }

    Matrix4x4 IConstantMember<Matrix4x4>.Value {
        set => WorldMatrix = value;
    }
}
