using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Graphics.Animation;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Veldrid;

namespace SCPCB.Graphics;

public interface IModelLoader {
    OwningModelTemplate LoadModel(GraphicsDevice gfx) => new(ExtractMeshes(gfx));
    OwningPhysicsModelTemplate LoadModelWithCollision(GraphicsDevice gfx, PhysicsResources physics, bool hullCollision = true) {
        (ICBShape Shape, Vector3 Offset) coll = hullCollision ? ExtractCollisionHull(physics) : (ExtractCollisionMesh(physics), Vector3.Zero);
        return new(ExtractMeshes(gfx), coll.Shape, coll.Offset);
    }

    IReadOnlyList<IMeshMaterial> ExtractMeshes(GraphicsDevice gfx);
    (ICBShape<ConvexHull>, Vector3 OffsetFromCenter) ExtractCollisionHull(PhysicsResources physics);
    ICBShape<Mesh> ExtractCollisionMesh(PhysicsResources physics);
}

// We're separating these because in practice we require specific vertex types in our implementation (those supporting bone weights).
public interface IAnimatedModelLoader : IModelLoader {
    OwningAnimatedModelTemplate LoadAnimatedModel(GraphicsDevice gfx);
}
