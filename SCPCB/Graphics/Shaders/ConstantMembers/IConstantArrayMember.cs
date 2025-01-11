using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IConstantArrayMember<T> : IConstantMember<T> where T : IConstantMember<T> {
    static abstract int Length { get; }
}

public interface IConstantArrayMember<T, TVal> : IConstantArrayMember<T> where TVal : unmanaged, IEquatable<TVal> where T : IConstantMember<T> {
    public Span<TVal> Values { get; }
}

public interface IBoneTransformsConstantMember : IConstantArrayMember<IBoneTransformsConstantMember, Matrix4x4> {
    public const int LENGTH = 64;
    static int IConstantArrayMember<IBoneTransformsConstantMember>.Length => LENGTH;
    public Span<Matrix4x4> BoneTransforms { get; }
    Span<Matrix4x4> IConstantArrayMember<IBoneTransformsConstantMember, Matrix4x4>.Values => BoneTransforms;
}
