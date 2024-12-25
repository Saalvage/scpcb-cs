using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Utility;

namespace SCPCB.Graphics.ModelCollections;

/// <summary>
/// Intended as the manager for the instance constants of a collection of models.
/// </summary>
public class ModelCollection : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, I3DModelHolder {
    IEnumerable<I3DModel> I3DModelHolder.Models => Models;
    public IReadOnlyList<I3DModel> Models { get; }

    private class Model3D : I3DModel {
        private readonly ModelCollection _coll;

        public Model3D(ModelCollection coll, ICBModel model) {
            _coll = coll;
            Model = model;
        }

        public Vector3 Position => _coll.WorldTransform.Position;
        public ICBModel Model { get; }
        public bool IsOpaque { get; }
    }

    public virtual Transform WorldTransform { get; set; } = new();

    protected virtual Transform GetUsedTransform(double interpolation) => WorldTransform;

    public ModelCollection(IReadOnlyList<ICBModel> meshes) {
        foreach (var mesh in meshes.DistinctBy(x => x.Constants)) {
            mesh.ConstantProviders.Add(this);
        }

        Models = meshes.Select(x => (I3DModel)new Model3D(this, x)).ToArray();
    }

    public Matrix4x4 GetValue(float interp) => GetUsedTransform(interp).GetMatrix();
}
