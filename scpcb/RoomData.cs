using BepuPhysics.Collidables;
using scpcb.Graphics.Primitives;

namespace scpcb;

public record RoomData(ICBModel[] Meshes, Mesh CollisionMesh);
