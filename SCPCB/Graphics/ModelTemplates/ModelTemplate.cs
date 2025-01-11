using SCPCB.Utility;

namespace SCPCB.Graphics.ModelTemplates;

// This hierarchy is rather complex, but I think we've gotten to a point where it accurately captures usage patterns.
// The main issue is that we have one template which holds all its resources (returned from the model loader),
// and we want to have others that derive from it. The issue is keeping the original template alive when
// using derivatives.
public interface IModelTemplate {
    IReadOnlyList<IMeshMaterial> Meshes { get; }
    ModelTemplate CreateDerivative();
}

// Necessary to instantiate models from non-owned resources that don't derive from a model template.
// Mustn't be instantiated from an owning model template.
public record ModelTemplate(IReadOnlyList<IMeshMaterial> Meshes) : IModelTemplate {
    public ModelTemplate CreateDerivative() => this;
}

public class OwningModelTemplate : Disposable, IModelTemplate {
    public IReadOnlyList<IMeshMaterial> Meshes { get; }

    public OwningModelTemplate(IReadOnlyList<IMeshMaterial> meshes) {
        Meshes = meshes;
    }

    public ModelTemplate CreateDerivative() => new DependantModelTemplate(this);

    protected override void DisposeImpl() {
        foreach (var (mesh, _) in Meshes) {
            mesh.Dispose();
        }
    }
}

public record DependantModelTemplate(IModelTemplate Dependent, IReadOnlyList<IMeshMaterial> Meshes) : ModelTemplate(Meshes) {
    public DependantModelTemplate(IModelTemplate dependent) : this(dependent, dependent.Meshes) { }
}
