using System.Text.Json;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Scenes;
using Serilog;

namespace scpcb.Serialization; 

public static class SerializationHelper {
    // TODO: For some reason it doesn't like it when we reuse this..
    // I assume the reason is the changing number of 
    public static readonly JsonSerializerOptions _opt = new() {
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
        return JsonSerializer.Deserialize<IEnumerable<BaseSerializableData>>(data, _opt).Select(x => x.Deserialize(gfxRes, scene, new ReferenceResolver()));
    }
}
