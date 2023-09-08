using System.Text.Json;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Scenes;
using Serilog;

namespace scpcb.Serialization; 

public static class SerializationHelper {
    private static readonly JsonSerializerOptions _opt = new() {
        TypeInfoResolver = SerializableDataTypeResolver.Instance,
        IncludeFields = true,
        WriteIndented = true,
    };

    public static string SerializeTest(IEnumerable<ISerializableEntity> entities) {
        var time = DateTimeOffset.UtcNow;
        var ret = JsonSerializer.Serialize(entities.Select(x => x.Serialize()).ToArray(), _opt);
        Log.Information("Serialization took {0} ms", (DateTimeOffset.UtcNow - time).TotalMilliseconds);
        return ret;
    }

    public static IEnumerable<IEntity> DeserializeTest(string data, GraphicsResources gfxRes, IScene scene) {
        using var refResolver = new ReferenceResolver();
        foreach (var d in JsonSerializer.Deserialize<IEnumerable<SerializableData>>(data, _opt)!) {
            yield return d.Deserialize(gfxRes, scene, refResolver);
        }
    }
}
