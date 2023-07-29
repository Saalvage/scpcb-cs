using scpcb.Collision;
using scpcb.Graphics;

namespace scpcb.RoomProviders;

public record RoomInfo(ICBMesh[] Meshes, CollisionMesh[] CollisionMeshes);
