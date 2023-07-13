using scpcb.Shaders;
using Veldrid;

namespace scpcb.Graphics;

public class TestAssimpMaterial : AssimpMaterial<ModelShader.Vertex> {
    protected override ModelShader.Vertex ConvertVertex(Model.SuperVertex vert) {
        return new(vert.Position, vert.TexCoords[0].XY());
    }

    public TestAssimpMaterial(GraphicsDevice gfx, ICBShader<ModelShader.Vertex> shader, params ICBTexture[] textures) : base(gfx, shader, null, textures) {
        
    }
}
