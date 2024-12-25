using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IUIProjectionMatrixConstantMember : IConstantMember<IUIProjectionMatrixConstantMember, Matrix4x4> {
    public Matrix4x4 ProjectionMatrix { get; set; }

    Matrix4x4 IConstantMember<IUIProjectionMatrixConstantMember, Matrix4x4>.Value {
        set => ProjectionMatrix = value;
    }
}
