using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Shaders.ConstantMembers;
using Veldrid;

namespace scpcb.Graphics; 

public class Model : IUpdatable, IEntity {
    private readonly ICBMesh[] _meshes;

    public virtual Transform WorldTransform { get; set; } = new();

    protected virtual Transform GetUsedTransform(double interpolation) => WorldTransform;

    public Model(params ICBMesh[] meshes) {
        _meshes = meshes;
    }

    public virtual void Render(CommandList commands, double interpolation) {
        var matrix = GetUsedTransform(interpolation).GetMatrix();
        foreach (var mesh in _meshes) {
            mesh.Material.Shader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(matrix);
            mesh.Render(commands);
        }
    }
}
