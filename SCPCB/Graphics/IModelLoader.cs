using System.Numerics;
using SCPCB.Graphics.Animation;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Veldrid;

namespace SCPCB.Graphics;

public interface IModelLoader {
    OwningModelTemplate LoadModel(GraphicsDevice gfx) => new(ExtractMeshes(gfx));
    OwningPhysicsModelTemplate LoadModelWithCollision(GraphicsDevice gfx, PhysicsResources physics) {
        var (shape, offset) = ExtractCollisionShape(physics);
        return new(ExtractMeshes(gfx), shape, offset);
    }

    IReadOnlyList<IMeshMaterial> ExtractMeshes(GraphicsDevice gfx);
    (ICBShape, Vector3 OffsetFromCenter) ExtractCollisionShape(PhysicsResources physics);
}

// We're separating these because in practice we require specific vertex types in our implementation (those supporting bone weights).
public interface IAnimatedModelLoader : IModelLoader {
    OwningAnimatedModelTemplate LoadAnimatedModel(GraphicsDevice gfx);
}
