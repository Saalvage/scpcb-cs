using Assimp;
using scpcb.Shaders;
using Veldrid;
using scpcb;

namespace scpcb.Graphics;

public class TestAssimpMaterial : AssimpMaterial<ModelShader.Vertex> {


    protected override ModelShader.Vertex ConvertVertex(Model.SuperVertex vert) {
        return new(vert.Position, vert.TexCoords[0].XY());
    }
}
