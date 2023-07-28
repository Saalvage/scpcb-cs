namespace scpcb.Graphics.Shaders.ConstantMembers; 

public interface IConstantMember { }

public interface IConstantMember<TImpl, T> : IConstantMember where T : unmanaged where TImpl : IConstantMember<TImpl, T> {
    public T Value { set; }
}
