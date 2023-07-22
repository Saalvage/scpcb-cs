using System.Numerics;
using Veldrid;

namespace scpcb.Graphics.UI; 

public class UIMesh : Disposable {
    private readonly UIShader _shader;
    private readonly CBMesh<UIShader.Vertex> _mesh;

    public Vector2 Position;
    public Vector2 Scale = Vector2.One;

    public UIMesh(GraphicsDevice gfx, UIShader shader, ICBTexture texture) {
        _shader = shader;
        _mesh = new(gfx, shader.CreateMaterial(texture),
            new UIShader.Vertex[] {
                new(new(-0.5f * texture.Width, 0.5f * texture.Height), new(0, 0)),
                new(new(0.5f * texture.Width, 0.5f * texture.Height), new(1, 0)),
                new(new(-0.5f * texture.Width, -0.5f * texture.Height), new(0, 1)),
                new(new(0.5f * texture.Width, -0.5f * texture.Height), new(1, 1)),
            },
            new uint[] {2, 1, 0, 1, 2, 3});
    }

    public void Render(CommandList commands) {
        _shader.VertexConstants.Position = Position;
        _shader.VertexConstants.Scale = Scale;
        _mesh.Render(commands);
    }

    protected override void DisposeImpl() {
        _mesh.Dispose();
    }
}