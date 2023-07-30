using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Shaders.ConstantMembers;
using Veldrid;

namespace scpcb.Graphics; 

public class Model : IUpdatable, IEntity {
    private readonly ICBMesh[] _meshes;
    private readonly ICBShader[] _shaders;

    public virtual Transform WorldTransform { get; set; } = new();

    protected virtual Transform GetUsedTransform(double interpolation) => WorldTransform;

    public Model(params ICBMesh[] meshes) {
        _meshes = meshes;
        _shaders = _meshes
            .Select(x => x.Material.Shader)
            .Distinct()
            .ToArray();
    }

    public virtual void Render(CommandList commands, double interpolation) {
        var matrix = GetUsedTransform(interpolation).GetMatrix();
        foreach (var shader in _shaders) {
            shader.SetConstantValue<IWorldMatrixConstantMember, Matrix4x4>(matrix);
        }
        foreach (var mesh in _meshes) {
            
            mesh.Render(commands);
        }
    }
}
