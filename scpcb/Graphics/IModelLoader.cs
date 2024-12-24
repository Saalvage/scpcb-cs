using BepuPhysics.Collidables;
using System.Numerics;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Veldrid;

namespace SCPCB.Graphics;

public interface IModelLoader {
    (IMeshMaterial[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}

public interface IModelLoader<TVertex> : IModelLoader where TVertex : unmanaged {
    (IMeshMaterial[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) IModelLoader.LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file)
        => LoadMeshes(gfx, physics, file);
    (IMeshMaterial<TVertex>[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}
