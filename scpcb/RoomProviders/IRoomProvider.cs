using scpcb.Graphics;
using scpcb.Physics;

namespace scpcb.RoomProviders;

public interface IRoomProvider {
    /// <summary>
    /// All lowercase file extensions supported by this provider without leading dot.
    /// </summary>
    public string[] SupportedExtensions { get; }

    public RoomData LoadRoom(GraphicsResources gfxRes, PhysicsResources physRes, string path);
}
