using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Graphics.Assimp;

/// <summary>
/// Because <see cref="AssimpVertex"/> is a ref struct, we can't use it as a generic type parameter to <see cref="IVertexConvertible{TOut,TIn}"/>.
/// </summary>
/// <typeparam name="TOut"></typeparam>
public interface IAssimpVertexConvertible<TOut> where TOut : unmanaged, IAssimpVertexConvertible<TOut> {
    public static abstract TOut ConvertVertex(AssimpVertex vert);
}
