using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Utility;

namespace SCPCB.Graphics.Models;

public class Model : Disposable, ISortableMeshInstanceHolder, IConstantProvider<IWorldMatrixConstantMember, Matrix4x4> {
    private class ModelMeshInstance : ISortableMeshInstance {
        private readonly Model _model;
        public Vector3 Position => _model.WorldTransform.Position;
        public IMeshInstance MeshInstance { get; }
        public bool IsOpaque => true;

        public ModelMeshInstance(Model model, IMeshInstance instance) {
            _model = model;
            MeshInstance = instance;
        }
    }

    IEnumerable<ISortableMeshInstance> ISortableMeshInstanceHolder.Models => Models;
    public IReadOnlyList<ISortableMeshInstance> Models { get; }

    // Keep alive.
    private readonly ICBModelTemplate _template;

    public virtual Transform WorldTransform { get; set; }

    public Model(ICBModelTemplate template) {
        _template = template;
        Models = template.Meshes.Instantiate().Select(x => new ModelMeshInstance(this, x)).ToArray();
        foreach (var mesh in Models.DistinctBy(x => x.MeshInstance.ConstantProviders)) {
            mesh.MeshInstance.ConstantProviders.Add(this);
        }
    }

    public virtual Matrix4x4 GetValue(float interp) => WorldTransform.GetMatrix();

    protected override void DisposeImpl() {
        foreach (var mi in Models) {
            mi.MeshInstance.Constants?.Dispose();
        }
    }
}
