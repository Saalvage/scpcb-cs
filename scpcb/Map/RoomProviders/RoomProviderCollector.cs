using scpcb.Graphics;
using scpcb.Physics;

namespace scpcb.Map.RoomProviders;

public class RoomProviderCollector {
    private readonly List<IRoomProvider> _providers = new();

    public RoomProviderCollector() {
        _providers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.ExportedTypes
                .Where(x => x.GetInterfaces().Any(x => x == typeof(IRoomProvider)))
                .Select(x => x.GetConstructor(Array.Empty<Type>()))
                .Where(x => x is not null))
            .Select(x => (IRoomProvider)x!.Invoke(null))
            .ToList();
    }

    public IRoomData LoadRoom(GraphicsResources gfxRes, PhysicsResources physRes, string path) {
        var ext = Path.GetExtension(path)[1..];
        var provider = _providers.FirstOrDefault(x => x.SupportedExtensions.Contains(ext));
        if (provider == null) {
            throw new ArgumentException($"No provider found for extension {ext}!");
        }

        return provider.LoadRoom(gfxRes, physRes, path);
    }

    public void RegisterProvider(IRoomProvider provider) {
        _providers.Add(provider);
    }
}
