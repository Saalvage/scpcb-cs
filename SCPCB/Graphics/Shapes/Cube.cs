using SCPCB.Graphics.Shaders.Vertices;

namespace SCPCB.Graphics.Shapes;

public class Cube : IShape {
    private static VPositionTexture[] _vertices = [
        // Front.
        new(new(-0.5f, -0.5f, 0.5f), new(0, 1)),
        new(new(0.5f, -0.5f, 0.5f), new(1, 1)),
        new(new(-0.5f, 0.5f, 0.5f), new(0, 0)),
        new(new(0.5f, 0.5f, 0.5f), new(1, 0)),
        // Back.
        new(new(0.5f, -0.5f, -0.5f), new(0, 1)),
        new(new(-0.5f, -0.5f, -0.5f), new(1, 1)),
        new(new(0.5f, 0.5f, -0.5f), new(0, 0)),
        new(new(-0.5f, 0.5f, -0.5f), new(1, 0)),
        // Left.
        new(new(-0.5f, -0.5f, -0.5f), new(0, 1)),
        new(new(-0.5f, -0.5f, 0.5f), new(1, 1)),
        new(new(-0.5f, 0.5f, -0.5f), new(0, 0)),
        new(new(-0.5f, 0.5f, 0.5f), new(1, 0)),
        // Right.
        new(new(0.5f, -0.5f, 0.5f), new(0, 1)),
        new(new(0.5f, -0.5f, -0.5f), new(1, 1)),
        new(new(0.5f, 0.5f, 0.5f), new(0, 0)),
        new(new(0.5f, 0.5f, -0.5f), new(1, 0)),
        // Top.
        new(new(-0.5f, 0.5f, 0.5f), new(0, 1)),
        new(new(0.5f, 0.5f, 0.5f), new(1, 1)),
        new(new(-0.5f, 0.5f, -0.5f), new(0, 0)),
        new(new(0.5f, 0.5f, -0.5f), new(1, 0)),
        // Bottom.
        new(new(-0.5f, -0.5f, -0.5f), new(0, 1)),
        new(new(0.5f, -0.5f, -0.5f), new(1, 1)),
        new(new(-0.5f, -0.5f, 0.5f), new(0, 0)),
        new(new(0.5f, -0.5f, 0.5f), new(1, 0)),
    ];
    public static ReadOnlySpan<VPositionTexture> Vertices => _vertices;

    private static uint[] _indices
        = [0, 1, 2, 3, 2, 1, 4, 5, 6, 7, 6, 5, 8, 9, 10, 11, 10, 9, 12, 13, 14, 15, 14, 13, 16, 17, 18, 19, 18, 17, 20, 21, 22, 23, 22, 21];
    public static ReadOnlySpan<uint> Indices => _indices;
}
