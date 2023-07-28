using Assimp;
using scpcb.Graphics.Shaders;

namespace scpcb.Graphics.Assimp;

public class TestAssimpMeshConverter : AssimpMeshConverter<ModelShader.Vertex> {
    private readonly ICBMaterial<ModelShader.Vertex> _mat;

    public TestAssimpMeshConverter(ICBMaterial<ModelShader.Vertex> mat) {
        _mat = mat;
    }

    protected override ModelShader.Vertex ConvertVertex(AssimpVertex vert)
        => new(vert.Position, vert.TexCoords[0].XY());

    protected override ICBMaterial<ModelShader.Vertex> ConvertMaterial(Material mat)
        => _mat;
}
