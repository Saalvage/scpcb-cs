using scpcb.Graphics;
using scpcb.Physics;

namespace scpcb.Map.RoomProviders;

public interface IRoomProvider {
    /// <summary>
    /// All lowercase file extensions supported by this provider without leading dot.
    /// </summary>
    public IEnumerable<string> SupportedExtensions { get; }

    public RoomData LoadRoom(GraphicsResources gfxRes, PhysicsResources physics, string path);
}
