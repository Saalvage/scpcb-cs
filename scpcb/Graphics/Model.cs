using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Shaders.ConstantMembers;
using Veldrid;

namespace scpcb.Graphics; 

public class Model : IUpdatable, IEntity {
    private readonly ICBMesh _mesh;

    public Transform WorldTransform { get; set; } = new();

    public Model(ICBMesh mesh) {
        _mesh = mesh;
    }

    public void Render(CommandList commands, double interpolation) {
        _mesh.Material.Shader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(WorldTransform.GetMatrix());
        _mesh.Render(commands);
    }
}
