using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers; 

public interface IWorldMatrixConstantMember : IConstantMember<IWorldMatrixConstantMember, Matrix4x4> {
    public Matrix4x4 WorldMatrix { get; set; }

    Matrix4x4 IConstantMember<IWorldMatrixConstantMember, Matrix4x4>.Value {
        get => WorldMatrix;
        set => WorldMatrix = value;
    }
}
