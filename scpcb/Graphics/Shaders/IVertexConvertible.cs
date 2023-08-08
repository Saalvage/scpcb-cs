namespace scpcb.Graphics.Shaders;

public interface IVertexConvertible<TOut, TIn> where TOut : unmanaged, IVertexConvertible<TOut, TIn> {
    static abstract TOut ConvertVertex(TIn vert);
}
