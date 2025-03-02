using SCPCB.Entities;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Utility;

namespace SCPCB.Graphics.Models;

public abstract class BaseModel : Disposable, ISortableMeshInstanceHolder, ITransformable {
    IEnumerable<ISortableMeshInstance> ISortableMeshInstanceHolder.Instances => Sortables;
    public IReadOnlyList<ISortableMeshInstance> Sortables { get; }

    // Keep alive.
    public IModelTemplate Template { get; }

    public abstract Transform WorldTransform { get; set; }

    protected BaseModel(IModelTemplate template) {
        Template = template;
        Sortables = template.Meshes.Instantiate().Select(x => new DependentSortableMeshInstance(x, this, true)).ToArray();
        foreach (var mesh in Sortables) {
            mesh.MeshInstance.ConstantProviders.Add(this);
        }
    }

    public virtual Transform GetInterpolatedWorldTransform(float interp) => WorldTransform;

    protected override void DisposeImpl() {
        foreach (var mi in Sortables) {
            mi.MeshInstance.Constants?.Dispose();
        }
    }
}

// This exists because it should be parentable, while the physics models (which inherit a lot of functionality) should not.
public class Model : BaseModel, IParentableTransformable {
    public Model(IModelTemplate template) : base(template) { }
    public ITransformable? Parent { get; set; }
    public Transform LocalTransform { get; set; } = new();
    
    // TODO: It sucks that we have to duplicate this from the default interface methods, but I don't think there's a better solution.
    public override Transform WorldTransform {
        get => (Parent?.WorldTransform ?? new Transform()) + LocalTransform;
        set => LocalTransform = value - (Parent?.WorldTransform ?? new Transform());
    }

    public override Transform GetInterpolatedWorldTransform(float interp)
        => (Parent?.GetInterpolatedWorldTransform(interp) ?? new Transform()) + LocalTransform;
}
