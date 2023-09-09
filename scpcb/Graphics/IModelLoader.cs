using BepuPhysics.Collidables;
using scpcb.Physics;
using Veldrid;

namespace scpcb.Graphics;

public interface IModelLoader {
    (IMeshMaterial[] Models, ConvexHull Collision) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}

public interface IModelLoader<TVertex> : IModelLoader where TVertex : unmanaged {
    (IMeshMaterial[] Models, ConvexHull Collision) IModelLoader.LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file)
        => LoadMeshes(gfx, physics, file);
    (IMeshMaterial<TVertex>[] Models, ConvexHull Collision) LoadMeshes(GraphicsDevice gfx, PhysicsResources physics, string file);
}
