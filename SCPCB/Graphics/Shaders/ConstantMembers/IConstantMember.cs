namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface IConstantMember<T> where T : IConstantMember<T>;

public interface IConstantMember<T, TVal> : IConstantMember<T> where TVal : unmanaged where T : IConstantMember<T, TVal> {
    public TVal Value { get; set; }
}
