using scpcb.Graphics;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Map.RoomProviders;

public class RoomProviderCollector {
    private readonly List<IRoomProvider> _providers = new();

    public RoomProviderCollector() {
        _providers = Helpers.GetAllLoadedTypes()
                .Where(x => x.GetInterfaces().Any(x => x == typeof(IRoomProvider)))
                .Select(x => x.GetConstructor(Array.Empty<Type>()))
                .Where(x => x is not null)
            .Select(x => (IRoomProvider)x!.Invoke(null))
            .ToList();
    }

    public IRoomData LoadRoom(IScene scene, GraphicsResources gfxRes, PhysicsResources physRes, string path) {
        var ext = Path.GetExtension(path)[1..];
        var provider = _providers.FirstOrDefault(x => x.SupportedExtensions.Contains(ext));
        if (provider == null) {
            throw new ArgumentException($"No provider found for extension {ext}!");
        }

        return provider.LoadRoom(scene, gfxRes, physRes, path);
    }

    public void RegisterProvider(IRoomProvider provider) {
        _providers.Add(provider);
    }
}
