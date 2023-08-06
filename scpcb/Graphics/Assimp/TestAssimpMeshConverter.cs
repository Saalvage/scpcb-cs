using Assimp;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Utility;
using ShaderGen;

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
