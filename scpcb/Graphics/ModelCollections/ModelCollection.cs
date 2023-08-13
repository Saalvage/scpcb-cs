using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Utility;

namespace scpcb.Graphics.ModelCollections;

/// <summary>
/// Intended as the manager for the instance constants of a collection of models.
/// </summary>
public class ModelCollection : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, IUpdatable, IEntity {
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

    public ModelCollection(IReadOnlyList<ICBModel> meshes) {
        foreach (var mesh in meshes) {
            mesh.ConstantProviders.Add(this);
        }

        Models = meshes.Select(x => (I3DModel)new Model3D(this, x)).ToArray();
    }

    public Matrix4x4 GetValue(float interp) => GetUsedTransform(interp).GetMatrix();
}
