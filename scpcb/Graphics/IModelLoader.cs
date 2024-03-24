using BepuPhysics.Collidables;
using scpcb.Physics;
using scpcb.Physics.Primitives;
using System.Numerics;
using Veldrid;

namespace scpcb.Graphics;

public interface IModelLoader {
    (IMeshMaterial[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}

public interface IModelLoader<TVertex> : IModelLoader where TVertex : unmanaged {
    (IMeshMaterial[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) IModelLoader.LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file)
        => LoadMeshes(gfx, physics, file);
    (IMeshMaterial<TVertex>[] Models, ICBShape<ConvexHull> Collision, Vector3 CenterOffset) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}
