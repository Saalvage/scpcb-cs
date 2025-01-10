using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics;
using Veldrid;

namespace SCPCB.Graphics;

public interface IModelLoader {
    OwningModelTemplate LoadMeshes(GraphicsDevice gfx, string file);
    OwningPhysicsModelTemplate LoadMeshesWithCollision(GraphicsDevice gfx, PhysicsResources physics, string file);
}
