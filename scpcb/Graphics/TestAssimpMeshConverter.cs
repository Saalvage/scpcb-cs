using scpcb.Graphics.Shaders;

namespace scpcb.Graphics;

public class TestAssimpMeshConverter : AssimpMeshConverter<ModelShader.Vertex> {
    protected override ModelShader.Vertex ConvertVertex(AssimpVertex vert) {
        return new(vert.Position, vert.TexCoords[0].XY());
    }
}
