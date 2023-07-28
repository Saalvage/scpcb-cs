namespace scpcb.Graphics.Shaders.ConstantMembers; 

public interface IConstantMember { }

public interface IConstantMember<T> : IConstantMember where T : unmanaged {
    public T Value { set; }
}
