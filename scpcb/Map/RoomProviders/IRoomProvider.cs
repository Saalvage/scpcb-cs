using scpcb.Graphics;
using scpcb.Physics;
using scpcb.Scenes;

namespace scpcb.Map.RoomProviders;

public interface IRoomProvider {
    /// <summary>
    /// All lowercase file extensions supported by this provider without leading dot.
    /// </summary>
    public IEnumerable<string> SupportedExtensions { get; }

    public IRoomData LoadRoom(IScene scene, GraphicsResources gfxRes, PhysicsResources physics, string path);
}
