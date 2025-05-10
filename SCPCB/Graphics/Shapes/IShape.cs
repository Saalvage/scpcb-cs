using SCPCB.Graphics.Shaders.Vertices;

namespace SCPCB.Graphics.Shapes;

public interface IShape {
    static abstract ReadOnlySpan<VPositionTexture> Vertices { get; }
    static abstract ReadOnlySpan<uint> Indices { get; }
}
