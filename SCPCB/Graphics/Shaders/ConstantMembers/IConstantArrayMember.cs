using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IConstantArrayMember {
    static abstract int Length { get; }
}

public interface IConstantArrayMember<TVal> : IConstantArrayMember where TVal : unmanaged {
    public Span<TVal> Values { get; }
}

public interface IBoneTransformsConstantMember : IConstantArrayMember<Matrix4x4> {
    public const int LENGTH = 64;
    static int IConstantArrayMember.Length => LENGTH;
    public Span<Matrix4x4> BoneTransforms { get; }
    Span<Matrix4x4> IConstantArrayMember<Matrix4x4>.Values => BoneTransforms;
}
