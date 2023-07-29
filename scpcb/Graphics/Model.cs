using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Shaders.ConstantMembers;
using Veldrid;

namespace scpcb.Graphics; 

public class Model : IUpdatable, IEntity {
    private readonly ICBMesh[] _meshes;

    // TODO: Add matrix caching.
    public Transform WorldTransform { get; set; } = new();

    public Model(params ICBMesh[] meshes) {
        _meshes = meshes;
    }

    // TODO: Add support for interpolation.
    public void Render(CommandList commands, double interpolation) {
        foreach (var mesh in _meshes) {
            mesh.Material.Shader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(WorldTransform.GetMatrix());
            mesh.Render(commands);
        }
    }
}
