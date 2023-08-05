using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Utility;

namespace scpcb.Graphics.ModelCollections;

/// <summary>
/// Intended as the manager for the instance constants of a collection of models.
/// </summary>
public class ModelCollection : IUpdatable, IEntity {
    private readonly ICBModel[] _models;
    private readonly IConstantHolder[] _constants;

    public IReadOnlyList<I3DModel> Models { get; }

    private class Model3D : I3DModel {
        private readonly ModelCollection _coll;

        public Model3D(ModelCollection coll, ICBModel model) {
            _coll = coll;
            Model = model;
        }

        public Vector3 Position => _coll.WorldTransform.Position;
        public ICBModel Model { get; }
    }

    public virtual Transform WorldTransform { get; set; } = new();

    protected virtual Transform GetUsedTransform(double interpolation) => WorldTransform;

    public ModelCollection(params ICBModel[] meshes) {
        _models = meshes;
        _constants = meshes
            .Select(x => x.Constants)
            .Where(x => x?.HasConstant<IWorldMatrixConstantMember>() ?? false)
            .Distinct()
            .ToArray()!;
        Models = _models.Select(x => (I3DModel)new Model3D(this, x)).ToArray();
    }

    public virtual void UpdateConstants(RenderTarget target, float interp) {
        var matrix = GetUsedTransform(interp).GetMatrix();
        foreach (var constants in _constants) {
            constants.SetValue<IWorldMatrixConstantMember, Matrix4x4>(matrix);
        }
    }
}
