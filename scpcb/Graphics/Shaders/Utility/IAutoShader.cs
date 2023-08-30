namespace scpcb.Graphics.Shaders.Utility;

/// <summary>
/// DO NOT implement this interface, instead, implement the parameterized one.
/// </summary>
/// <remarks>
/// This type exists solely for internal use and has to exist due to C# limitations.
/// </remarks>
public interface IAutoShader {
    static abstract Type VertexBlockType { get; }
    static abstract Type FragmentBlockType { get; }
    static abstract Type InstanceVertexBlockType { get; }
    static abstract Type InstanceFragmentBlockType { get; }

    static virtual ShaderParameters DefaultParameters => ShaderParameters.Default;
}

public interface IAutoShader<TVertexConstants, TFragmentConstants, TInstanceVertexConstants, TInstanceFragmentConstants> : IAutoShader 
        where TVertexConstants : unmanaged where TFragmentConstants : unmanaged
        where TInstanceVertexConstants : unmanaged where TInstanceFragmentConstants : unmanaged {

    static Type IAutoShader.VertexBlockType => typeof(TVertexConstants);
    static Type IAutoShader.FragmentBlockType => typeof(TFragmentConstants);
    static Type IAutoShader.InstanceVertexBlockType => typeof(TInstanceVertexConstants);
    static Type IAutoShader.InstanceFragmentBlockType => typeof(TInstanceFragmentConstants);

    TVertexConstants VertexBlock { get; }
    TFragmentConstants FragmentBlock { get; }
    TInstanceVertexConstants InstanceVertexBlock { get; }
    TInstanceFragmentConstants InstanceFragmentBlock { get; }
}
