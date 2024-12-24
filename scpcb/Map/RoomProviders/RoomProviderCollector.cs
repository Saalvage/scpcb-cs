using SCPCB.Graphics;
using SCPCB.Physics;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Map.RoomProviders;

public class RoomProviderCollector {
    private readonly List<IRoomProvider> _providers;

    public RoomProviderCollector() {
        _providers = Helpers.GetAllLoadedTypes()
                .Where(x => x.GetInterfaces().Any(x => x == typeof(IRoomProvider)))
                .Select(x => x.GetConstructor([]))
                .Where(x => x is not null)
            .Select(x => (IRoomProvider)x!.Invoke(null))
            .ToList();
        Log.Information("Available room providers: {providers}", _providers);
    }

    public IRoomData LoadRoom(IScene scene, GraphicsResources gfxRes, PhysicsResources physRes, string path) {
        var ext = Path.GetExtension(path)[1..];
        var provider = _providers.FirstOrDefault(x => x.SupportedExtensions.Contains(ext));
        if (provider == null) {
            throw new ArgumentException($"No provider found for extension {ext}!");
        }

        Log.Information("Loading room {path} with provider {provider}", path, provider);
        return provider.LoadRoom(scene, gfxRes, physRes, path);
    }

    public void RegisterProvider(IRoomProvider provider) {
        _providers.Add(provider);
    }
}
