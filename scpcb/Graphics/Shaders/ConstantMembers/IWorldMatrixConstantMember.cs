using System.Numerics;

namespace scpcb.Graphics.Shaders.ConstantMembers; 

public interface IWorldMatrixConstantMember : IConstantMember<IWorldMatrixConstantMember, Matrix4x4> {
    public Matrix4x4 WorldMatrix { get; set; }

    Matrix4x4 IConstantMember<IWorldMatrixConstantMember, Matrix4x4>.Value {
        set => WorldMatrix = value;
    }
}
