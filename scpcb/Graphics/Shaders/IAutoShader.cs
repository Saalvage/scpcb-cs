using scpcb.Graphics.Primitives;

namespace scpcb.Graphics.Shaders;

/// <summary>
/// DO NOT implement this interface, instead, implement the parameterized one.
/// </summary>
/// <remarks>
/// This type exists solely for internal use and has to exist due to C# limitations.
/// </remarks>
// By keeping this name we point potential implementors into the correct direction via the type constraint in the shader cache.
public interface IAutoShader {
    // Stupid shit but should get message across.
    // Why are unbound generic names not allowed in type constraints?? :(
    void DO_NOT_IMPLEMENT_THIS_INTERFACE_IMPLEMENT_GENERIC_INTERFACE_INSTEAD();

    static abstract ShaderParameters DefaultParameters { get; }
}

public interface IAutoShader<TVertexConstants, TFragmentConstants, TInstanceVertexConstants, TInstanceFragmentConstants> : IAutoShader 
        where TVertexConstants : unmanaged where TFragmentConstants : unmanaged
        where TInstanceVertexConstants : unmanaged where TInstanceFragmentConstants : unmanaged {
    void IAutoShader.DO_NOT_IMPLEMENT_THIS_INTERFACE_IMPLEMENT_GENERIC_INTERFACE_INSTEAD() { }

    static ShaderParameters IAutoShader.DefaultParameters => ShaderParameters.Default;
}

public interface IAutoShader<TVertexConstants, TFragmentConstants> : IAutoShader<TVertexConstants, TFragmentConstants, Empty, Empty>
    where TVertexConstants : unmanaged where TFragmentConstants : unmanaged { }
