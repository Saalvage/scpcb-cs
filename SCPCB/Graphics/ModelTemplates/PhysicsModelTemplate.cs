using System.Numerics;
using SCPCB.Physics.Primitives;

namespace SCPCB.Graphics.ModelTemplates;

public interface IPhysicsModelTemplate : IModelTemplate {
    ICBShape Shape { get; }
    Vector3 OffsetFromCenter { get; }

    ModelTemplate IModelTemplate.CreateDerivative() => CreateDerivative();
    new PhysicsModelTemplate CreateDerivative();
}

public record PhysicsModelTemplate(IReadOnlyList<IMeshMaterial> Meshes, ICBShape Shape, Vector3 OffsetFromCenter)
    : ModelTemplate(Meshes), IPhysicsModelTemplate {
    public PhysicsModelTemplate CreateDerivative() => this;
}

public class OwningPhysicsModelTemplate : OwningModelTemplate, IPhysicsModelTemplate {
    public ICBShape Shape { get; }
    public Vector3 OffsetFromCenter { get; }

    public OwningPhysicsModelTemplate(IReadOnlyList<IMeshMaterial> meshes, ICBShape shape, Vector3 offset) : base(meshes) {
        Shape = shape;
        OffsetFromCenter = offset;
    }

    public PhysicsModelTemplate CreateDerivative() => new DependantPhysicsModelTemplate(this);

    protected override void DisposeImpl() {
        Shape.Dispose();
        base.DisposeImpl();
    }
}

public record DependantPhysicsModelTemplate(IPhysicsModelTemplate Dependent, IReadOnlyList<IMeshMaterial> Meshes, ICBShape Shape, Vector3 OffsetFromCenter)
    : PhysicsModelTemplate(Meshes, Shape, OffsetFromCenter) {
    public DependantPhysicsModelTemplate(IPhysicsModelTemplate dependent) : this(dependent, dependent.Meshes, dependent.Shape, dependent.OffsetFromCenter) { }
}
