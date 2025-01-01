using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers; 

public interface IViewMatrixConstantMember : IConstantMember<IViewMatrixConstantMember, Matrix4x4> {
    public Matrix4x4 ViewMatrix { get; set; }

    Matrix4x4 IConstantMember<IViewMatrixConstantMember, Matrix4x4>.Value {
        get => ViewMatrix;
        set => ViewMatrix = value;
    }
}
